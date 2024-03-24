Imports System.Data
Imports Microsoft.VisualBasic
Imports MySql.Data
Imports MySql.Data.MySqlClient

Public Class ClsEmployeesEvid


    Private cOrgSfr As String = ""


    Public Sub New()


        DtList = New DataTable

    End Sub




    Public Property DtList As DataTable
    Public Property DrDetails As DataRow

    ''' <summary>
    ''' Lista uposlenika - prema pravima koja ima logovani user (pEmployeeID)
    ''' </summary>
    ''' <param name="pEmployeeID"></param>
    ''' <returns></returns>
    Public Function getList(ByVal pDbTableName As String, ByVal pEmployeeID As Integer, Optional ByVal pOrgSfr As Integer = 1) As DataTable

        Dim cDbTableNamePrefix As String = "tmp_emp_"

        Dim myda As MySqlClient.MySqlDataAdapter
        Dim mycmd As MySqlCommand

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            mycmd = New MySqlCommand
            mycmd.Connection = myconnection

            Dim strSQL As String = <![CDATA[ 
CALL EMPwb_create_list('@DbTblSession', '@OrgSfr', @EmployeeID);
]]>.Value


            'mycmd.Parameters.Clear()
            strSQL = strSQL.Replace("@DbTblSession", pDbTableName.ToString)
            strSQL = strSQL.Replace("@OrgSfr", pOrgSfr.ToString)
            mycmd.CommandText = strSQL.Replace("@EmployeeID", pEmployeeID.ToString)

            'mycmd.Parameters.AddWithValue("@row_number", 0)

            DtList.Rows.Clear()
            myda = New MySqlClient.MySqlDataAdapter(mycmd)
            myda.Fill(DtList)

        End Using



        Return DtList

    End Function

    Public Function getRoleList(ByVal pDbTableName As String, ByVal pEmployeeID As Integer, Optional ByVal pOrgSfr As Integer = 1) As DataTable

        Dim cDbTableNamePrefix As String = "tmp_emp_"

        Dim myda As MySqlClient.MySqlDataAdapter
        Dim mycmd As MySqlCommand

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            mycmd = New MySqlCommand
            mycmd.Connection = myconnection

            Dim strSQL As String = <![CDATA[ 
CALL EMPwb_create_rolelist('@DbTblSession', '@OrgSfr', @EmployeeID);
]]>.Value


            'mycmd.Parameters.Clear()
            strSQL = strSQL.Replace("@DbTblSession", pDbTableName.ToString)
            strSQL = strSQL.Replace("@OrgSfr", pOrgSfr.ToString)
            mycmd.CommandText = strSQL.Replace("@EmployeeID", pEmployeeID.ToString)

            'mycmd.Parameters.AddWithValue("@row_number", 0)

            DtList.Rows.Clear()
            myda = New MySqlClient.MySqlDataAdapter(mycmd)
            myda.Fill(DtList)

        End Using



        Return DtList

    End Function

    Public Function getRoleListAPI(ByVal pDbTableName As String, ByVal pEmployeeID As Integer, Optional ByVal pOrgSfr As Integer = 1) As DataTable

        Dim cDbTableNamePrefix As String = "tmp_emp_"

        Dim myda As MySqlClient.MySqlDataAdapter
        Dim mycmd As MySqlCommand

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            mycmd = New MySqlCommand
            mycmd.Connection = myconnection

            Dim strSQL As String = <![CDATA[ 
CALL EMPwb_create_rolelistAPI('@DbTblSession', '@OrgSfr', @EmployeeID);
]]>.Value


            'mycmd.Parameters.Clear()
            strSQL = strSQL.Replace("@DbTblSession", pDbTableName.ToString)
            strSQL = strSQL.Replace("@OrgSfr", pOrgSfr.ToString)
            mycmd.CommandText = strSQL.Replace("@EmployeeID", pEmployeeID.ToString)

            'mycmd.Parameters.AddWithValue("@row_number", 0)

            DtList.Rows.Clear()
            myda = New MySqlClient.MySqlDataAdapter(mycmd)
            myda.Fill(DtList)

        End Using



        Return DtList

    End Function

    Public Function getOrgRoleList(ByVal pDbTableName As String, ByVal pEmployeeID As Integer, Optional ByVal pOrgSfr As Integer = 1) As DataTable

        Dim cDbTableNamePrefix As String = "tmp_emp_"

        Dim myda As MySqlClient.MySqlDataAdapter
        Dim mycmd As MySqlCommand

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            mycmd = New MySqlCommand
            mycmd.Connection = myconnection

            Dim strSQL As String = <![CDATA[ 
CALL EMPwb_create_orgrolelist('@DbTblSession', '@OrgSfr', @EmployeeID);
]]>.Value


            'mycmd.Parameters.Clear()
            strSQL = strSQL.Replace("@DbTblSession", pDbTableName.ToString)
            strSQL = strSQL.Replace("@OrgSfr", pOrgSfr.ToString)
            mycmd.CommandText = strSQL.Replace("@EmployeeID", pEmployeeID.ToString)
            mycmd.ExecuteNonQuery()

            strSQL = <![CDATA[ 
CALL EMPwb_create_orgrolelist_pom('@DbTblSession');
]]>.Value


            'mycmd.Parameters.Clear()
            mycmd.CommandText = strSQL.Replace("@DbTblSession", pDbTableName.ToString)

            DtList.Rows.Clear()
            myda = New MySqlClient.MySqlDataAdapter(mycmd)
            myda.Fill(DtList)

        End Using



        Return DtList

    End Function

    Public Sub saveEmail(ByVal pEmployeeID As Integer, ByVal pEmail As String)

        Dim myconnection As MySqlClient.MySqlConnection
        Dim mycmd As MySqlCommand

        myconnection = New MySqlClient.MySqlConnection(ConnectionString)
        myconnection.Open()

        Try
            mycmd = New MySqlCommand
            mycmd.Connection = myconnection

            Dim strSQL As String = <![CDATA[ 
UPDATE evd_employees_lpd
SET
  Email_pos = @Email_pos
WHERE EmployeeID = @EmployeeID;
    ]]>.Value


            mycmd.CommandText = strSQL

            mycmd.Parameters.AddWithValue("@Email_pos", pEmail)
            mycmd.Parameters.AddWithValue("@EmployeeID", pEmployeeID)

            mycmd.Prepare()

            mycmd.ExecuteNonQuery()
        Catch ex As Exception

        End Try



        myconnection.Close()
    End Sub

    Public Sub saveWebPristup(ByVal pEmployeeID As Integer, ByVal pWebAccess As Boolean)

        Dim myconnection As MySqlClient.MySqlConnection
        Dim mycmd As MySqlCommand

        Dim pWebAccessInt As Integer = 1 ' zabranjen prisup webu: IsLockedOut = 1 
        If pWebAccess Then pWebAccessInt = 0 ' dozvoljen prisup webu: IsLockedOut = 0

        myconnection = New MySqlClient.MySqlConnection(ConnectionString)
        myconnection.Open()

        Try
            mycmd = New MySqlCommand
            mycmd.Connection = myconnection

            Dim strSQL As String = <![CDATA[ 
UPDATE `my_aspnet_membership` m, `my_aspnet_users_to_employees` u2e
SET m.`IsLockedOut` = @pWebAccess
WHERE u2e.`users_id`=m.`userId` AND u2e.`employees_id`= @EmployeeID;
    ]]>.Value


            mycmd.CommandText = strSQL

            mycmd.Parameters.AddWithValue("@pWebAccess", pWebAccessInt)
            mycmd.Parameters.AddWithValue("@EmployeeID", pEmployeeID)

            mycmd.Prepare()

            mycmd.ExecuteNonQuery()
        Catch ex As Exception

        End Try



        myconnection.Close()
    End Sub

    Public Sub createUser(ByVal pEmployeeID As Integer, ByVal pEmail As String)

        Dim newUserName As String = pEmail
        Dim newUserPass As String = defUserPefix & pEmployeeID.ToString

        Membership.CreateUser(newUserName, newUserPass, pEmail)

        Dim newUserId As Integer = GetUserId(newUserName)
        If newUserId > -1 And newUserId <> 0 Then
            Roles.AddUserToRole(newUserName, "uposlenik")
            setUserToEmployee(newUserId, pEmployeeID)
        Else
            Exit Sub
        End If

    End Sub

    Public Function resetPassword(ByVal pEmail As String) As String

        Dim newPassword As String = ""

        Dim userName As String = Membership.GetUserNameByEmail(pEmail)
        newPassword = Membership.GetUser(userName).ResetPassword()

        Return newPassword

    End Function

    Public Sub addRole(ByVal pUserId As Integer, ByRef pRoleToAdd As String(), ByRef pRoleToRemove As String())


        Dim _userName As String = GetUserName(pUserId)

        If _userName IsNot Nothing Then
            For Each el In pRoleToRemove
                Dim _allUserRoles() As String = Roles.GetRolesForUser(_userName)

                If _allUserRoles.Contains(el) Then
                    Roles.RemoveUserFromRole(_userName, el)
                End If
            Next

            For Each el In pRoleToAdd
                Dim _allUserRoles() As String = Roles.GetRolesForUser(_userName)

                If Not _allUserRoles.Contains(el) Then
                    Roles.AddUserToRole(_userName, el)
                End If
            Next


            'Roles.AddUserToRole(_userName, pRole)
        Else
            Exit Sub
        End If

    End Sub

    Private Function GetUserId(ByVal pUserName As String) As Integer

        Dim mycmd As MySql.Data.MySqlClient.MySqlCommand
        Dim mydr As MySql.Data.MySqlClient.MySqlDataReader

        Dim strSQL As String = <![CDATA[ 
SELECT id FROM my_aspnet_users WHERE NAME=@pUserName AND applicationId = 3;
    ]]>.Value

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            mycmd = New MySqlCommand
            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL

            mycmd.Parameters.AddWithValue("@pUserName", pUserName)
            mycmd.Prepare()

            mydr = mycmd.ExecuteReader
            While mydr.Read
                Try
                    Return mydr.Item("id")
                Catch ex As Exception
                End Try
            End While
            mydr.Close()
        End Using

        Return -1

    End Function

    Private Function GetUserName(ByVal pUserId As Integer) As String

        Dim mycmd As MySql.Data.MySqlClient.MySqlCommand
        Dim mydr As MySql.Data.MySqlClient.MySqlDataReader

        Dim strSQL As String = <![CDATA[ 
SELECT name FROM my_aspnet_users WHERE id=@pUserId AND applicationId = 3;
    ]]>.Value

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            mycmd = New MySqlCommand
            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL

            mycmd.Parameters.AddWithValue("@pUserId", pUserId)
            mycmd.Prepare()

            mydr = mycmd.ExecuteReader
            While mydr.Read
                Try
                    Return mydr.Item("name")
                Catch ex As Exception
                End Try
            End While
            mydr.Close()
        End Using

        Return Nothing

    End Function

    Private Sub setUserToEmployee(ByVal pUserId As Integer, ByVal pEmployeeID As Integer)

        Dim mycmd As MySql.Data.MySqlClient.MySqlCommand

        Dim strSQL As String = <![CDATA[ 
UPDATE `evd_employees` e
SET e.`User`=@pUserId WHERE e.`EmployeeID`=@pEmployeeId;
    ]]>.Value

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            mycmd = New MySqlCommand
            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL

            mycmd.Parameters.AddWithValue("@pUserId", pUserId)
            mycmd.Parameters.AddWithValue("@pEmployeeId", pEmployeeID)
            mycmd.Prepare()

            mycmd.ExecuteNonQuery()

        End Using
    End Sub

    Public Sub setOrgRoleToEmployee(ByVal pOrgIds As String, ByVal pEmail As String)

        Dim mycmd As MySql.Data.MySqlClient.MySqlCommand

        Dim strSQL As String = <![CDATA[ 
    ]]>.Value

        Select Case checkOrgRoleToEmployee(pEmail)
            Case True
                strSQL = <![CDATA[ 
UPDATE sys_usr_role_json
SET `Name` = @pOrgIds 
WHERE `Type` = @pEmail AND Form='Organizacija2';
    ]]>.Value
            Case False
                strSQL = <![CDATA[ 
INSERT INTO `sys_usr_role_json` (
 `Name`, `Enabled`, `Type`, `Form`
)
VALUES
  (
    @pOrgIds, 'True', @pEmail, 'Organizacija2'
  );
    ]]>.Value
        End Select


        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            mycmd = New MySqlCommand
            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL

            mycmd.Parameters.AddWithValue("@pOrgIds", pOrgIds)
            mycmd.Parameters.AddWithValue("@pEmail", pEmail)
            mycmd.Prepare()

            Try
                mycmd.ExecuteNonQuery()
            Catch ex As Exception

            End Try


        End Using
    End Sub

    Public Sub setOrgRoleToEmployeeAPI(ByVal pOrgIds As String, ByVal pEmail As String)

        Dim mycmd As MySql.Data.MySqlClient.MySqlCommand

        Dim strSQL As String = <![CDATA[ 
    ]]>.Value

        Select Case checkOrgRoleToEmployeeAPI(pEmail)
            Case True
                strSQL = <![CDATA[ 
UPDATE sys_usr_role_json
SET `Name` = @pOrgIds 
WHERE `Type` = @pEmail AND Form='APIputnal';
    ]]>.Value
            Case False
                strSQL = <![CDATA[ 
INSERT INTO `sys_usr_role_json` (
 `Name`, `Enabled`, `Type`, `Form`
)
VALUES
  (
    @pOrgIds, 'True', @pEmail, 'APIputnal'
  );
    ]]>.Value
        End Select


        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            mycmd = New MySqlCommand
            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL

            mycmd.Parameters.AddWithValue("@pOrgIds", pOrgIds)
            mycmd.Parameters.AddWithValue("@pEmail", pEmail)
            mycmd.Prepare()

            Try
                mycmd.ExecuteNonQuery()
            Catch ex As Exception

            End Try


        End Using
    End Sub

    Private Function checkOrgRoleToEmployee(ByVal pEmail As String) As Boolean

        Dim mycmd As MySql.Data.MySqlClient.MySqlCommand
        Dim mydr As MySql.Data.MySqlClient.MySqlDataReader

        Dim _hasRows As Boolean = False

        Dim strSQL As String = <![CDATA[ 
SELECT  `Name` As Num
FROM sys_usr_role_json
WHERE `Type` = @pEmail AND Form='Organizacija2';
    ]]>.Value

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            mycmd = New MySqlCommand
            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL

            mycmd.Parameters.AddWithValue("@pEmail", pEmail)
            mycmd.Prepare()

            mydr = mycmd.ExecuteReader
            _hasRows = mydr.HasRows

            'While mydr.Read
            '    Try

            '    Catch ex As Exception
            '    End Try
            'End While
            mydr.Close()
        End Using

        Return _hasRows

    End Function

    Private Function checkOrgRoleToEmployeeAPI(ByVal pEmail As String) As Boolean

        Dim mycmd As MySql.Data.MySqlClient.MySqlCommand
        Dim mydr As MySql.Data.MySqlClient.MySqlDataReader

        Dim _hasRows As Boolean = False

        Dim strSQL As String = <![CDATA[ 
SELECT  `Name` As Num
FROM sys_usr_role_json
WHERE `Type` = @pEmail AND Form='APIputnal';
    ]]>.Value

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            mycmd = New MySqlCommand
            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL

            mycmd.Parameters.AddWithValue("@pEmail", pEmail)
            mycmd.Prepare()

            mydr = mycmd.ExecuteReader
            _hasRows = mydr.HasRows

            'While mydr.Read
            '    Try

            '    Catch ex As Exception
            '    End Try
            'End While
            mydr.Close()
        End Using

        Return _hasRows

    End Function
End Class
