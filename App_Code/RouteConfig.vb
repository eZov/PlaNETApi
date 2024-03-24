Imports System.Web.Http
Imports System.Web.Http.Cors
Imports System.Web.Routing
Imports Microsoft.AspNet.FriendlyUrls

Public Module RouteConfig
    Sub RegisterRoutes(ByVal routes As RouteCollection)

        routes.EnableFriendlyUrls()

        'Dim cors As New EnableCorsAttribute("*", "*", "*")
        'GlobalConfiguration.Configuration.EnableCors(cors)

        ' Web API routes
        '
        GlobalConfiguration.Configuration.MessageHandlers.Add(New TokenValidationHandler())

        GlobalConfiguration.Configuration.Routes.MapHttpRoute(name:="DefaultApi", routeTemplate:="api/{controller}/{action}/{id}", defaults:=New With {
Key .id = RouteParameter.[Optional]
})

        '        GlobalConfiguration.Configuration.Routes.MapHttpRoute(name:="DefaultApi", routeTemplate:="api/{controller}/{action}/{id}", defaults:=New With {
        'Key .id = RouteParameter.[Optional]
        '})
        '


    End Sub
End Module
