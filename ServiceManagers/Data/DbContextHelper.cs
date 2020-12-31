//Copyright(c) 2017 Jon P Smith
//https://github.com/JonPSmith/EfCore.TestSupport/tree/master/TestSupport
//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files (the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions:

//The above copyright notice and this permission notice shall be included in all
//copies or substantial portions of the Software.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Rocky.ServiceManagers.Data
{
    public static class DbContextHelper
    {
        /// <summary>
        /// This will ensure an empty database by using the WipeAllDataFromDatabase method
        /// </summary>
        /// <param name="context"></param>
        /// <param name="addBracketsAndSchema">Optional: normally it only uses the table name, but for cases where you have multiple schemas,
        /// or a table name that needs brackets the you can set to to true. Default is false</param>
        /// <param name="maxDepth">Values to stop the wipe method from getting in a circular reference loop</param>
        /// <param name="excludeTypes">This allows you to provide the Types of the table that you don't want wiped. 
        /// Useful if you have a circular ref that WipeAllDataFromDatabase cannot handle. You then must wipe that part.</param>
        /// <returns>True if the database is created, false if it already existed.</returns>
        public static bool CreateEmptyViaWipe(this DbContext context,
            bool addBracketsAndSchema = false,
            int maxDepth = 10, 
            params Type[] excludeTypes)
        {
            var databaseWasCreated = context.Database.EnsureCreated();
            if (!databaseWasCreated)
                context.WipeAllDataFromDatabase(addBracketsAndSchema, maxDepth, excludeTypes);
            return databaseWasCreated;
        }

        /// <summary>
        /// This is a fast way to delete all the rows in all the tables in a database. 
        /// Useful for unit testing where you need an empty database.
        /// This will work out the right order to delete rows from tables to avoid a delete behavour of Restrict
        /// from causing problems. Its not perfect (circular references can cause it problems) but it will throw
        /// an exception if it cannot achieve a wipe of the database
        /// </summary>
        /// <param name="context"></param>
        /// <param name="addBracketsAndSchema">Optional: normally it only uses the table name, but for cases where you have multiple schemas,
        /// or a table name that needs brackets the you can set to to true. Default is false</param>
        /// <param name="maxDepth">Value to stop the wipe method from getting in a circular reference loop. Defaults to 10</param>
        /// <param name="excludeTypes">This allows you to provide the Types of the table that you don't want wiped. 
        /// Useful if you have a circular ref that WipeAllDataFromDatabase cannot handle. You then must wipe that part yourself.</param>
        public static void WipeAllDataFromDatabase(this DbContext context,
            bool addBracketsAndSchema = false,
            int maxDepth = 10, 
            params Type[] excludeTypes)
        {
            foreach (var tableName in context.GetTableNamesInOrderForWipe(addBracketsAndSchema, maxDepth, excludeTypes))
            {
                context.Database.ExecuteSqlRaw("DELETE FROM " + tableName);
            }
        }

        //NOTE: This will not handle a circular relationship: e.g. EntityA->EntityB->EntityA
        private static IEnumerable<string>
            GetTableNamesInOrderForWipe //#A
            (this DbContext context,
                bool addBracketsAndSchema,
                int maxDepth = 10,
                params Type[] excludeTypes) //#B
        {
            var allEntities = context.Model
                .GetEntityTypes()
                .Where(x => !excludeTypes.Contains(x.ClrType))
                .ToList(); //#C

            ThrowExceptionIfCannotWipeSelfRef(allEntities); //#D

            var principalsDict = allEntities //#E
                .SelectMany(x => x.GetForeignKeys()
                    .Where(y => !excludeTypes.Contains(y.PrincipalEntityType.ClrType))
                    .Select(y => y.PrincipalEntityType)).Distinct()
                .ToDictionary(k => k, v => //#F
                    v.GetForeignKeys()
                        .Where(y => y.PrincipalEntityType != v) //#G
                        .Select(y => y.PrincipalEntityType).ToList()); //#H

            var result = allEntities //#I
                .Where(x => !principalsDict.ContainsKey(x)) //#I
                .ToList(); //#I

            /************************************************************************
            #A This method looks at the relationships and returns the tables names in the right order to wipe all their rows without incurring a foreign key delete constraint
            #B You can exclude entity classes that you need to handle yourself, for instance - any references that only contain circular references
            #C This gets the IEntityType for all the entities, other than those that were excluded. This contains the information on how each table is built, with its relationships
            #D This contains a check for the hierarchical (entity that references itself) case where an entity refers to itself - if the delete behavior of this foreign key is set to restrict then you cannot simply delete all the rows in one go
            #E I extract all the principal entities from the entities we are considering ...
            #F ... And put them in a dictionary, with the IEntityType being the key
            #G ... I remove any self reference links as these are automatically handled
            #H ... And the PrincipalEntityType being the value 
            #I I start the list of entities to delete by putting all the dependent entities first, as I must delete the rows in these tables first, and the order doesn't matter
            ************************************************************/

            var reversePrincipals = new List<IEntityType>(); //#A
            int depth = 0; //#B
            while (principalsDict.Keys.Any()) //#C
            {
                foreach (var principalNoLinks in
                    principalsDict
                        .Where(x => !x.Value.Any()).ToList()) //#D
                {
                    reversePrincipals.Add(principalNoLinks.Key); //#E
                    principalsDict
                        .Remove(principalNoLinks.Key); //#F
                    foreach (var removeLink in
                        principalsDict.Where(x =>
                            x.Value.Contains(principalNoLinks.Key))) //#G
                    {
                        removeLink.Value
                            .Remove(principalNoLinks.Key); //#H
                    }
                }
                if (++depth >= maxDepth) //#I
                    ThrowExceptionMaxDepthReached(
                        principalsDict.Keys.ToList(), depth);
            }
            reversePrincipals.Reverse(); //#J
            result.AddRange(reversePrincipals); //#K

            var dbFacade = new DatabaseFacade(context); //#L

            return result.Select(x => 
                FormTableNameWithSchema(x, addBracketsAndSchema, dbFacade)); //#M
        }

        private static string FormTableNameWithSchema(IEntityType entityType, bool addBracketsAndSchema, DatabaseFacade dbFacade)
        {
            if (!addBracketsAndSchema)
                return entityType.GetTableName();

            var nameParts = new List<string>();

            if (entityType.GetSchema() != null)
                nameParts.Add(entityType.GetSchema());

            nameParts.Add(entityType.GetTableName());

            return dbFacade.IsSqlServer() ? 
                $@"[{string.Join(@"].[", nameParts)}]" :
                $@"""{string.Join(@""".""", nameParts.Select(ToSnakeCase))}""";
        }

        private static string ToSnakeCase(string str) 
        {
            Regex pattern = new Regex(@"[A-Z]{2,}(?=[A-Z][a-z]+[0-9]*|\b)|[A-Z]?[a-z]+[0-9]*|[A-Z]|[0-9]+");
            return string.Join("_", pattern.Matches(str)).ToLower();
        }
        
        private static void ThrowExceptionIfCannotWipeSelfRef(List<IEntityType> allEntities)
        {
            var cannotWipes = allEntities //#B
                .SelectMany(x => x.GetForeignKeys()         //#B
                    .Where(y => y.PrincipalEntityType == x      //#B
                                && (y.DeleteBehavior == DeleteBehavior.Restrict
                                    || y.DeleteBehavior == DeleteBehavior.ClientSetNull)))
                .ToList(); //#B
            if (cannotWipes.Any())
                throw new InvalidOperationException(
                    "You cannot delete all the rows in one go in entity(s): " +
                    string.Join(", ", cannotWipes.Select(x => x.DeclaringEntityType.Name)));
        }

        private static void ThrowExceptionMaxDepthReached(List<IEntityType> principalsDictKeys, int maxDepth)
        {
            throw new InvalidOperationException(
                $"It looked to a depth of {maxDepth} and didn't finish. Possible circular reference?\nentity(s) left: " +
                string.Join(", ", principalsDictKeys.Select(x => x.ClrType.Name)));
        }
    }
}