# Jannesen.Web

This middleware is running in IIS:
- Converting HTTP request to SQL Server storedprocedure calls.
- Service static content for single page applicant.
- Basic framework for extenions to run in IIS that are part of total application (Logging, resource config, database access, etc).


## Jannesen.Web.Core

- Read jannesen.config files.
- Load required assembly.
- map http request to handler.
- Standard error handler that returns a text or json.
- Logging


## Jannesen.Web.MSSql

- Translate http call into sql server stored procedure calls.
- Querystring, header variable and body (variable) are translate to stored procedure argument.
- The XML data received from sql server is translated to json response.
- Also a storeprocedure kan return raw (binary) data.
- Support HTTP compression in request and response.


## Jannesen.Web.StaticFile

- For serving static application files (like javascript, css, etc). Not for serving big amounts of data.
- Special for single page application. Standard way for cache poisoning.
- Compress the response if posibble.
- All data is cache in memory. So extremly fast. 


## Jannesen.Web.ExcelExport

- Querystring, header variable and body (variable) are translate to stored procedure argument (Uses Jannesen.Web.MSSql for this).
- The table response in translated to a Excel sheet.

## web.config

Add the follow in web.config to install this middle ware.

``` xml
<?xml version="1.0" encoding="UTF-8"?>
<configuration>
	<system.webServer>
		<modules runAllManagedModulesForAllRequests="true">
			<add name="JannesenWebAppl" type="Jannesen.Web.Core.ApplModule, Jannesen.Web.Core" preCondition="managedHandler" />
		</modules>
		<handlers>
			<clear />
			<add name="JannesenWebHandlerNotFound"         path="*.config" verb="*"                   type="Jannesen.Web.Core.HandlerNotFound, Jannesen.Web.Core" />
			<add name="PageHandlerFactory-Integrated-4.0"  path="*.aspx"   verb="GET,HEAD,POST,DEBUG" type="System.Web.UI.PageHandlerFactory"                    preCondition="integratedMode,runtimeVersionv4.0" />
			<add name="JannesenWebHandlerFactory-CSS"      path="*.css"    verb="*"                   type="Jannesen.Web.Core.HandlerFactory, Jannesen.Web.Core" preCondition="integratedMode,runtimeVersionv4.0" />
			<add name="JannesenWebHandlerFactory-HTML"     path="*.html"   verb="*"                   type="Jannesen.Web.Core.HandlerFactory, Jannesen.Web.Core" preCondition="integratedMode,runtimeVersionv4.0" />
			<add name="JannesenWebHandlerFactory-JS"       path="*.js"     verb="*"                   type="Jannesen.Web.Core.HandlerFactory, Jannesen.Web.Core" preCondition="integratedMode,runtimeVersionv4.0" />
			<add name="JannesenWebHandlerFactory-SPJ"      path="*.spj"    verb="*"                   type="Jannesen.Web.Core.HandlerFactory, Jannesen.Web.Core" preCondition="integratedMode,runtimeVersionv4.0" />
		</handlers>
	</system.webServer>
</configuration>
```

## jannesen.config

At the first request the directory tree is scanned for jannesen.config files. 

Syntax:
```xml
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
    <name name="Site name"        />
    <load name="assemble name"    />
    
    <include file="filename"   />

    <http-handler path="*.html"     verb="GET"      type="staticfile"   mimetype="text/html; charset=utf-8"                                             public="1" compress="1" cache-max-age="0"                               />
    <http-handler path="*.css"      verb="GET"      type="staticfile"   mimetype="text/css; charset=utf-8"                                              public="1" compress="1" cache-max-age="0" version-cache-max-age="86400" />
    <http-handler path="*.js"       verb="GET"      type="staticfile"   mimetype="application/javascript; charset=utf-8"                                public="1" compress="1" cache-max-age="0" version-cache-max-age="86400" />
</configuration>
```


```xml
    <name name="Site name"        />
```
Name of the site

```xml
    <load name="assemble name"    />
```
Load a assemble

```xml
    <include file="filename"   />
```
Include file

```xml
    <http-handler path="*.html"     verb="GET"      type="staticfile"   mimetype="text/html; charset=utf-8"                                             public="1" compress="1" cache-max-age="0"                               />
```
Static file handler.

|attr      | description
|:---------|:-----
| path     | path of request
| verb     | verb of request
| type     | "staticfile"
| mimetype | mimetype returned
| public   | Public file the file is access using the IIS POOL account. When a file is public the data is cached in memory.
| compress | Compress the response.
| cache-max-age | cache max-age returnd 
| version-cache-max-age | cache max-age returnd when the request was xxxx?v=version. This way the file is cached by the browser for max-age time and don't needs to refresh the file.


```xml
	<http-handler path="net/account:lookup.spj" verb="GET" type="sql-json2" procedure="dbo.[intranet-tas/net/account:lookup.spj:GET]" database="TAS2">
		<parameter name="key" type="int" source="querystring:key" />
		<response responsemsg="Y7A8nXmH2AJQrRQDK3UIwwZRJvM=" type="object:mandatory">
			<field name="id" type="int" />
			<field name="username" type="varchar(256)" />
			<field name="fullname" type="varchar(100)" />
		</response>
	</http-handler>
```

|attr       | description
|:----------|:-----
| path      | path of request
| verb      | verb of request
| type      | "sql-json2"
| procedure | the sql server stored procedure to call
| database  | Database resource.
| parameter.name   | The name of the stored procedure argument.
| parameter.type   | native sql server type.
| parameter.source | where the get the value. querystring, body, cookie, formdata, textjson, urlpath
| response | how the translate the xml return by the stored procedure to json.

## Remark

The TypedTSql transpiler is capable to generate file that kan by used by this middle ware. So most of the entry are generated.