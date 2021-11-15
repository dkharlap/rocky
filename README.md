
The main problem solved is 



- Simplified working with the app
- Allows using scope when verifying results. It allows to mock objects otherwise created by middle ware during normal execution.
- Db migrations run once and only once at startup
- Fast data cleanup between tests


WebAppConfig usually you will have an inherited WebAppConfigBase that would serve as 
a base for all configs in your app. 

The main purpose for service descriptors is to describe and add dependency into service collection
