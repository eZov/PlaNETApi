<%@ Application Language="VB" %>
<%@ Import Namespace="System.Web.Routing" %>
<%@ Import Namespace="System.Web.Http" %>
<%@ Import Namespace="System.Net.Http.Headers" %>
<%@ Import Namespace="System.Io" %>

<script runat="server">

    Sub Application_Start(ByVal sender As Object, ByVal e As EventArgs)
        ' Code that runs on application startup
        'AuthConfig.RegisterOpenAuth()

        'Syncfusion Licensing Register
        Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("MjgxNTQ4QDMxMzgyZTMyMmUzMGJaQXdFTlhINFp4VG0zMVRCUlBFMUloNmFIR2xybThmdG92dS9qREpNbDg9;MjgxNTQ5QDMxMzgyZTMyMmUzMFozdEg3eWUzd0VjNjg3dXVEOWZ4TmtTQnNZc2QrcFlTZWgxTWxyVEhWNUU9;MjgxNTUwQDMxMzgyZTMyMmUzMExqaURjZVMwM29COWNCNVFmdXY3aCtEb3p2blpRQ2VpZUlyMnczNzJmeWs9")

        'Dim sessionId As String = Session.SessionID
        RouteConfig.RegisterRoutes(RouteTable.Routes)

        GlobalConfiguration.Configuration.Formatters.JsonFormatter.SupportedMediaTypes.Add(New MediaTypeHeaderValue("multipart/form-data"))

    End Sub

    Protected Sub Application_BeginRequest(ByVal sender As Object, ByVal e As EventArgs)


        Dim currentContext As HttpContext = CType(sender, HttpApplication).Context


        If HttpContext.Current.Request.HttpMethod = "OPTIONS" Then
            'HttpContext.Current.Response.Headers.Add("Access-Control-Allow-Origin", "*")
            'HttpContext.Current.Response.Headers.Add("Access-Control-Allow-Methods", "GET,HEAD,PUT,PATCH,POST,DELETE")
            HttpContext.Current.Response.Headers.Add("Access-Control-Allow-Headers", "content-type, authorization, accept")
            'HttpContext.Current.Response.Headers.Add("Vary", "Origin, Access-Control-Request-Headers")
            HttpContext.Current.Response.Headers.Add("Content-Length", "0")
            HttpContext.Current.Response.Headers.Remove("Referrer-Policy")
        Else
            HttpContext.Current.Response.Headers.Add("Access-Control-Allow-Headers", "*")
        End If

        If HttpContext.Current.Request.HttpMethod = "OPTIONS" Then
            HttpContext.Current.Response.Flush()
            '
            ' Bez CompleteReequest IIS serevr nastavlja procesirati zahjtev te izbaci exception: 
            ' Session state has created a session id, but cannot save it because the response was already flushed by the application.
            ' što obori POST zahtjev nakon OPTIONS, jer browser misli da je dobio odgovor od servera
            '
            CompleteRequest()
        End If

        If HttpContext.Current.Request.Url.AbsoluteUri.Contains("api") Then
            'System.Web.HttpContext.Current.SetSessionStateBehavior(System.Web.SessionState.SessionStateBehavior.Disabled)
            currentContext.SetSessionStateBehavior(System.Web.SessionState.SessionStateBehavior.Disabled)
        End If

    End Sub

    Protected Sub Authenticate_Request(ByVal sender As Object, ByVal e As EventArgs)

    End Sub

    Protected Sub Authorize_Request(ByVal sender As Object, ByVal e As EventArgs)

    End Sub


    Protected Sub Resolve_Request_Cache(ByVal sender As Object, ByVal e As EventArgs)

    End Sub

    Protected Sub Map_Request_Handler(ByVal sender As Object, ByVal e As EventArgs)

    End Sub

    Protected Sub Application_EndRequest(ByVal sender As Object, ByVal e As EventArgs)

        If HttpContext.Current.Request.Url.AbsoluteUri.Contains("api") Then
            HttpContext.Current.Response.Headers.Remove("Set-Cookie")
        End If

    End Sub

    Sub Application_PostAuthorizeRequest()
        System.Web.HttpContext.Current.SetSessionStateBehavior(
      System.Web.SessionState.SessionStateBehavior.Required)
    End Sub

    Sub Application_End(ByVal sender As Object, ByVal e As EventArgs)
        ' Code that runs on application shutdown
    End Sub

    Sub Application_Error(ByVal sender As Object, ByVal e As EventArgs)
        ' Code that runs when an unhandled error occurs
    End Sub

    Public Sub Session_Start(ByVal sender As Object, ByVal e As EventArgs)

    End Sub

    'Sub Session_End(ByVal sender As Object, ByVal e As EventArgs)

    '    'Dim DeleteThis As String = Session.SessionID
    '    'Dim Files As String() = Directory.GetFiles(HttpContext.Current.Server.MapPath("~/Data/"))


    '    'For Each file__1 As String In Files
    '    '    If file__1.ToUpper().Contains(DeleteThis.ToUpper()) Then
    '    '        Dim file As New FileInfo(file__1)
    '    '        file.Delete()
    '    '    End If
    '    'Next

    'End Sub

    Sub Application_ResolveRequestCache(ByVal sender As Object, ByVal e As EventArgs)

        'If HttpContext.Current.Request.Url.AbsoluteUri.Contains("api") Then
        '    System.Web.HttpContext.Current.SetSessionStateBehavior(System.Web.SessionState.SessionStateBehavior.Disabled)
        '    Exit Sub
        'End If

    End Sub

    Sub Application_AcquireRequestState(ByVal sender As Object, ByVal e As EventArgs)

        '
        ' API Controller
        '
        If HttpContext.Current.Request.Url.AbsoluteUri.Contains("api") Then
            Exit Sub
        End If

        Dim _session = System.Web.HttpContext.Current.Session

        If ((_session IsNot Nothing) Or (String.IsNullOrWhiteSpace(_session.SessionID))) Then
            Exit Sub
        End If

        Dim userIsAuthenticated As Boolean = userIsAuthenticated = (User IsNot Nothing) Or (User.Identity IsNot Nothing) Or (User.Identity.IsAuthenticated = True)

        Logoff()

        If ((Not userIsAuthenticated) And (_session.SessionID.Equals(Session("__MyAppSession")))) Then

            ClearSession()

        End If

    End Sub

    Private Sub Logoff()
        FormsAuthentication.SignOut()

        Dim authCookie = New HttpCookie(FormsAuthentication.FormsCookieName, String.Empty) With {.Expires = DateTime.Now.AddYears(-1)}

        Response.Cookies.Add(authCookie)
        FormsAuthentication.RedirectToLoginPage()

    End Sub

    Private Sub ClearSession()

        Session.Abandon()

        Dim sessionCookie = New HttpCookie("ASP.NET_SessionId", String.Empty) With {.Expires = DateTime.Now.AddYears(-1)}
        Response.Cookies.Add(sessionCookie)

    End Sub
</script>