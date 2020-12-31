
The main problem solved is 



- Simplified working with the app
- Verification doesn't have scope nor middleware. We help managing this.
- Db migrations run once and only once at startup
- Fast data cleanup between tests


WebAppConfig usually you will have an inherited WebAppConfigBase that would serve as 
a base for all configs in your app. 

The main purpose for service descriptors is to describe and add dependency into service collection