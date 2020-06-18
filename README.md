# WebInvestigation
An Azure WebApp (.NET Core) program for PoC Azure private link. WebInvestigation will help you if you would like to try-and-error service connection from a Cloud SERVER.

![](https://aqtono.com/tomarika/webinvestigation/WebInvestigationIcon.png)  

WebInvestigation web application is for Azure PoC help utility.   

To use this, deploy WebInvestigation project to Azure WebApp.

## Functions

Function|Description
:--|:--
/api/Dns?host=localhost|Resolve IP address w/ host name.
/api/Ping?ip=127.0.0.1|Ping to IP address from the server.
/api/Dummy|A simple WebAPI to response back method name and body contents.
/Info|Show you Server information, Web Request and Response.<br/> You can confirm an outbound IP address from the server.
/Call|Call WebAPI from the server. You can choose method GET, POST and others.
/SqlServer|Try to connect to an Azure SQL Server from the server. You can use SQL command directly.



