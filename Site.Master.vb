Imports Microsoft.VisualBasic.ApplicationServices
Imports Syncfusion.JavaScript.Web

Partial Class SiteMaster
    Inherits MasterPage

    Const AntiXsrfTokenKey As String = "__AntiXsrfToken"
    Const AntiXsrfUserNameKey As String = "__AntiXsrfUserName"
    Dim _antiXsrfTokenValue As String

    'Dim currentUser As MembershipUser
    'Dim aspSession As ClsSessionSink

    Public Event DDLRolesChanged As CommandEventHandler

    Protected Sub Page_Init(sender As Object, e As System.EventArgs)
        ' The code below helps to protect against XSRF attacks
        Dim requestCookie As HttpCookie = Request.Cookies(AntiXsrfTokenKey)
        Dim requestCookieGuidValue As Guid
        If ((Not requestCookie Is Nothing) AndAlso Guid.TryParse(requestCookie.Value, requestCookieGuidValue)) Then
            ' Use the Anti-XSRF token from the cookie
            _antiXsrfTokenValue = requestCookie.Value
            Page.ViewStateUserKey = _antiXsrfTokenValue
        Else
            ' Generate a new Anti-XSRF token and save to the cookie
            _antiXsrfTokenValue = Guid.NewGuid().ToString("N")
            Page.ViewStateUserKey = _antiXsrfTokenValue

            Dim responseCookie As HttpCookie = New HttpCookie(AntiXsrfTokenKey) With {.HttpOnly = True, .Value = _antiXsrfTokenValue}
            If (FormsAuthentication.RequireSSL And Request.IsSecureConnection) Then
                responseCookie.Secure = True
            End If
            Response.Cookies.Set(responseCookie)
        End If

        AddHandler Page.PreLoad, AddressOf master_Page_PreLoad
    End Sub

    Private Sub master_Page_PreLoad(sender As Object, e As System.EventArgs)
        If (Not IsPostBack) Then
            ' Set Anti-XSRF token
            ViewState(AntiXsrfTokenKey) = Page.ViewStateUserKey
            ViewState(AntiXsrfUserNameKey) = If(Context.User.Identity.Name, String.Empty)
        Else
            ' Validate the Anti-XSRF token
            If (Not DirectCast(ViewState(AntiXsrfTokenKey), String) = _antiXsrfTokenValue _
                Or Not DirectCast(ViewState(AntiXsrfUserNameKey), String) = If(Context.User.Identity.Name, String.Empty)) Then
                Throw New InvalidOperationException("Validation of Anti-XSRF token failed.")
            End If
        End If
    End Sub


    Private Sub SiteMaster_Load(sender As Object, e As EventArgs) Handles Me.Load


        'If aspSession Is Nothing Then Exit Sub

        'Me.DropDownListRoles.DataSource = aspSession.AllRoles
        'DropDownListRoles.DataValueField = "id"
        'DropDownListRoles.DataTextField = "name"
        'DropDownListRoles.DataBind()

        'For Each el As Syncfusion.JavaScript.Web.DropDownListItem In DropDownListRoles.Items
        '    If el.Text = aspSession.CurrentRole Then
        '        el.Selected = True
        '    Else
        '        el.Selected = False
        '    End If
        'Next

        '_LabelUser.Text = "| Prijavljeni ste kao : " & aspSession.EmployeeName & " * Korisnicko ime : " & aspSession.UserName & "|"

        Dim _ngApiVersion As New ngApiVersion
        lblAPIver.Text = _ngApiVersion.getFooter()

    End Sub

    Protected Sub Unnamed_LoggedOut(sender As Object, e As EventArgs)

        FormsAuthentication.SignOut()
        Session.Abandon()
        'Dim authCookie As HttpCookie = New HttpCookie(FormsAuthentication.FormsCookieName, String.Empty) With {Expires = Date.Now.AddYears(-1)}
        'Response.Cookies.Add(authCookie)
        'Dim sessionCookie As HttpCookie = New HttpCookie("ASP.NET_SessionId", String.Empty) With {Expires = DateTime.Now.AddYears(-1)}
        'Response.Cookies.Add(sessionCookie)
        FormsAuthentication.RedirectToLoginPage()

    End Sub

End Class

