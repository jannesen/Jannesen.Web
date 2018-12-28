# Jannesen.Web

This is middle for converting HTTP request to Sql Server storedprocedure calls, service static data for single page application etc. This middleware is running in IIS.

IIS Middleware for:
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


## Jannesen.Web.StaticFile

- Querystring, header variable and body (variable) are translate to stored procedure argument (Uses Jannesen.Web.MSSql for this).
- The table response in translated to a Excel sheet.