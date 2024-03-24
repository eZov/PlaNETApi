
Partial Class _Default
    Inherits Page

    Dim currentUser As MembershipUser
    Dim aspSession As ClsSessionSink

    Dim APIname As String = "PlaNET - API"
    Dim APIver As String = "ver 0.9.22.0203"

    Dim myMasterPage As SiteMaster = CType(Page.Master, SiteMaster)

    Private Sub _Default_Load(sender As Object, e As EventArgs) Handles Me.Load

        aspSession = DirectCast(Session("SessionSink"), ClsSessionSink)
        myMasterPage = CType(Page.Master, SiteMaster)


        Dim _ngApiVersion As New ngApiVersion
        lblAPIver.Text = _ngApiVersion.getMainText()
    End Sub

    Private Sub _Default_Init(sender As Object, e As EventArgs) Handles Me.Init
        'AddHandler myMasterPage.DDLRolesChanged, AddressOf DropDownListRoles_ValueSelect
    End Sub

End Class