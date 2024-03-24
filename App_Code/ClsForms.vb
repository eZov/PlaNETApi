Imports Microsoft.VisualBasic
Imports System
Imports System.Diagnostics
Imports System.Web.Profile
Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports Syncfusion.JavaScript.Web


Public Class ClsForms


    'Private Sub DropDownListRoles_Init(ByRef myMasterPage As SiteMaster, ByRef aspSession As ClsSessionSink)

    '    If myMasterPage.mstDDLRoles.Visible Then
    '        myMasterPage.mstDDLRoles.DataSource = aspSession.AllRoles
    '        myMasterPage.mstDDLRoles.DataValueField = "id"
    '        myMasterPage.mstDDLRoles.DataTextField = "name"
    '        myMasterPage.mstDDLRoles.DataBind()

    '        If myMasterPage.mstDDLRoles.SelectedIndex = -1 Then
    '            Dim idx As Integer = 0
    '            For Each el As Syncfusion.JavaScript.Web.DropDownListItem In myMasterPage.mstDDLRoles.Items

    '                If el.Text = aspSession.UserRole And myMasterPage.mstDDLRoles.SelectedIndex = -1 Then
    '                    myMasterPage.mstDDLRoles.SelectedIndex = idx
    '                    Dim roleMenu As Syncfusion.JavaScript.Web.Menu = myMasterPage.mstMenu

    '                    aspSession.GetMenu(aspSession.UserRole, roleMenu)
    '                End If

    '                idx += 1
    '            Next
    '        End If

    '    End If

    'End Sub

End Class
