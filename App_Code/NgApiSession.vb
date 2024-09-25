Imports System.Data
Imports System.Globalization
Imports System.IdentityModel.Tokens.Jwt
Imports System.Security.Claims
Imports System.Web.Configuration
Imports Microsoft.IdentityModel.Tokens
Imports Microsoft.VisualBasic
Imports MySql.Data
Imports MySql.Data.MySqlClient
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Serialization

Public Class NgApiSession

    Public Property Email As String
    Public Property UserId As Integer = -1
    Public Property IsApproved As Boolean = False
    Public Property IsLockedOut As Boolean = True
    Public Property UUID As String = ""                     ' UUID za mPerun

    Public Property Verified As Boolean = False

    Public Property EmployeeID As Integer = -1              ' EmpId uposlenika - usera koji je prijavljen, ako ima
    Public Property Role As String = ""                     ' Role - usera koji je prijavljen

    Public Property UserRoles As New List(Of String)

    Public Property WorkStation As Integer = -1     ' Šema ispunjvanja evidencije: 0 - korisnik unosi evidencije, 1 - rukovodilac unosi, korisnik ima samo ReadOnly

    Public Property CurrentEmployeeId As Integer = -1       ' EmpId uposlenika koga je izabrao user
    Public Property CurrentOrgSfr As String = ""            ' Sifra org.jed. koja je izabrana
    Public Property CurrentOrgId As Integer = -1            ' Id org.jed. koja je izabrana

    Public Function isUsernamePasswordValid(ByVal pPassword As String) As Boolean

        'Dim ConnectionString As String = ApiGlobal.domainConnectionString
        Dim ConnectionString As String = ApiGlobal.domainConnectionString

        Dim strSQL As String


        strSQL = <![CDATA[ 
SELECT * FROM `my_aspnet_membership` ms
WHERE ms.`Email`=@email AND ms.`Password`=@password;
    ]]>.Value


        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim dbRead As MySqlClient.MySqlDataReader
            Dim mycmd As New MySql.Data.MySqlClient.MySqlCommand

            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL
            mycmd.Parameters.AddWithValue("@email", Me.Email)
            mycmd.Parameters.AddWithValue("@password", pPassword)
            mycmd.Prepare()

            dbRead = mycmd.ExecuteReader
            Do While dbRead.Read
                Me.UserId = dbRead.GetInt16("userId")
                Me.IsApproved = dbRead.GetBoolean("IsApproved")
                Me.IsLockedOut = dbRead.GetBoolean("IsLockedOut")
            Loop
            dbRead.Close()

            If Me.IsApproved = True AndAlso Me.IsLockedOut = False Then

                getEmployeeId()
                getEmployeeRoles()

                Return True
            End If

        End Using

        Return False

    End Function

    ' isUUIDindbase (UUID)
    Public Function isUUIDindbase(ByVal pUUID As String) As Boolean

        Dim ConnectionString As String = ApiGlobal.domainConnectionString
        Dim strSQL As String
        Dim _hasRows As Boolean = False

        strSQL = <![CDATA[ 
SELECT u2u.`uuid`
FROM `my_aspnet_users_to_uuid` u2u 
INNER JOIN `my_aspnet_membership` m ON u2u.`users_id`=m.`userId`
WHERE m.`Email`=@email;
    ]]>.Value


        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim dbRead As MySqlClient.MySqlDataReader
            Dim mycmd As New MySql.Data.MySqlClient.MySqlCommand

            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL


            mycmd.Parameters.AddWithValue("@email", Me.Email)
            mycmd.Prepare()

            dbRead = mycmd.ExecuteReader
            If dbRead.HasRows Then
                _hasRows = True
                Do While dbRead.Read
                    Me.UUID = dbRead.GetString("uuid")
                Loop
            End If
            dbRead.Close()

        End Using

        Return _hasRows

    End Function


    ' insertUUID (UUID)
    Public Function insUUID(ByVal pUUID As String) As Boolean

        Dim ConnectionString As String = ApiGlobal.domainConnectionString
        Dim strSQL As String
        Dim _RowsAffected As Integer = -1

        strSQL = <![CDATA[
INSERT IGNORE INTO `my_aspnet_users_to_uuid` (`users_id`, `uuid`) 
VALUES (@pUserId, @pUUID); 

    ]]>.Value

        If pUUID.Length > 10 Then
            Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
                myconnection.Open()

                Dim mycmd As New MySql.Data.MySqlClient.MySqlCommand

                mycmd.Connection = myconnection

                mycmd.CommandText = strSQL


                mycmd.Parameters.AddWithValue("@pUUID", pUUID)
                mycmd.Parameters.AddWithValue("@pUserId", Me.UserId)
                mycmd.Prepare()

                Try
                    _RowsAffected = mycmd.ExecuteNonQuery()

                Catch ex As Exception
                    Console.WriteLine(ex.Message)
                    Return False
                End Try

            End Using
        End If


        Return If(_RowsAffected > 0, True, False)


    End Function


    Private Sub getEmployeeId()

        Dim ConnectionString As String = ApiGlobal.domainConnectionString
        Dim strSQL As String


        strSQL = <![CDATA[ 
SELECT m.`userId`, u2e.`employees_id`, IFNULL(ep.`work_station`,0) AS work_station 
FROM `my_aspnet_membership` m LEFT JOIN `my_aspnet_users_to_employees` u2e
ON m.`userId`=u2e.`users_id`
LEFT JOIN `evd_employees_pim` ep
ON u2e.`employees_id`=ep.`EmployeeID`
WHERE m.`Email`=@email;  
    ]]>.Value


        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim dbRead As MySqlClient.MySqlDataReader
            Dim mycmd As New MySql.Data.MySqlClient.MySqlCommand

            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL


            mycmd.Parameters.AddWithValue("@email", Me.Email)
            mycmd.Prepare()

            dbRead = mycmd.ExecuteReader
            Do While dbRead.Read
                Me.EmployeeID = dbRead.GetInt16("employees_id")
                Me.WorkStation = dbRead.GetInt16("work_station")
            Loop
            dbRead.Close()


        End Using


    End Sub

    Public Function getEmployeeId(ByVal pEmpEmail As String) As Integer

        Dim ConnectionString As String = ApiGlobal.domainConnectionString
        Dim strSQL As String
        Dim _employeeId As Integer = -1

        strSQL = <![CDATA[ 
SELECT m.`userId`, u2e.`employees_id` 
FROM `my_aspnet_membership` m LEFT JOIN `my_aspnet_users_to_employees` u2e
ON m.`userId`=u2e.`users_id`
WHERE m.`Email`=@email;  
    ]]>.Value


        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim dbRead As MySqlClient.MySqlDataReader
            Dim mycmd As New MySql.Data.MySqlClient.MySqlCommand

            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL


            mycmd.Parameters.AddWithValue("@email", pEmpEmail)
            mycmd.Prepare()

            dbRead = mycmd.ExecuteReader
            Do While dbRead.Read
                _employeeId = dbRead.GetInt16("employees_id")
            Loop
            dbRead.Close()


        End Using

        Return _employeeId

    End Function


    Public Sub getEmployeeRoles()

        Dim ConnectionString As String = ApiGlobal.domainConnectionString
        Dim strSQL As String


        strSQL = <![CDATA[ 
SELECT m.`userId`, u2r.`roleId`, r.`name`
FROM `my_aspnet_membership` m 
LEFT JOIN `my_aspnet_usersinroles` u2r
ON m.`userId`=(u2r.`userId`-1)
LEFT JOIN `my_aspnet_roles` r 
ON u2r.`roleId`=r.`id`
WHERE m.`Email`=@email
ORDER BY r.`name`;  
    ]]>.Value


        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim dbRead As MySqlClient.MySqlDataReader
            Dim mycmd As New MySql.Data.MySqlClient.MySqlCommand

            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL


            mycmd.Parameters.AddWithValue("@email", Me.Email)
            mycmd.Prepare()

            UserRoles.Clear()
            dbRead = mycmd.ExecuteReader
            Do While dbRead.Read
                UserRoles.Add(dbRead.GetString("name"))
            Loop
            dbRead.Close()


        End Using


    End Sub


    Public Sub getClaims(ByRef pClaimsPrincipal As ClaimsPrincipal)

        For Each el As Claim In pClaimsPrincipal.Claims

            Select Case el.Type
                Case ClaimTypes.Email
                    Me.Email = el.Value
                Case ClaimTypes.Role
                    Me.Role = el.Value
                Case "UserRoles"
                    Me.UserRoles.Add(el.Value)
                Case ClaimTypes.NameIdentifier
                    '
                    ' EmployeeID
                    Integer.TryParse(el.Value, Me.EmployeeID)
                Case "WorkStation"
                    Integer.TryParse(el.Value, Me.WorkStation)
            End Select
        Next


    End Sub

    '
    ' API methods block
    '

    Public Function CreateToken() As String

        Dim issuedAt As DateTime = DateTime.UtcNow
        Dim expires As DateTime = DateTime.UtcNow.AddDays(7)
        Dim tokenHandler = New JwtSecurityTokenHandler()

        Dim _claimLst As New List(Of Claim)

        _claimLst.Add(New Claim(ClaimTypes.Email, Me.Email))
        _claimLst.Add(New Claim(ClaimTypes.NameIdentifier, Me.EmployeeID.ToString))
        _claimLst.Add(New Claim(ClaimTypes.Role, Me.Role))
        _claimLst.Add(New Claim("SubRole", getSubRole(Me.Role, Me.Email)))

        For Each el In Me.UserRoles
            _claimLst.Add(New Claim("UserRoles", el))
        Next

        ' šema ispunjavanja evidencija
        _claimLst.Add(New Claim("WorkStation", Me.WorkStation.ToString))

        Dim claimsIdentity = New ClaimsIdentity(_claimLst)
        Const secrectKey As String = "your secret key goes here"
        Dim securityKey = New SymmetricSecurityKey(System.Text.Encoding.[Default].GetBytes(secrectKey))
        Dim signingCredentials = New SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature)
        Dim token = CType(tokenHandler.CreateJwtSecurityToken(issuer:="http://zov-consulting.ba/:8080/",
                                                              audience:="http://zov-consulting.ba/:8080/",
                                                              subject:=claimsIdentity,
                                                              notBefore:=issuedAt,
                                                              expires:=expires,
                                                              signingCredentials:=signingCredentials), JwtSecurityToken)
        Dim tokenString = tokenHandler.WriteToken(token)

        Return tokenString

    End Function

    Public Function getApiRolesAllowed(ByVal pController As String, ByVal pMethod As String) As String()

        Dim ConnectionString As String = ApiGlobal.domainConnectionString
        Dim strSQL As String


        strSQL = <![CDATA[ 
SELECT sj.Opis 
FROM `sys_json` sj
WHERE sj.`Naziv` = 'api.roles';  
    ]]>.Value

        Dim rawJson As String = ""

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim dbRead As MySqlClient.MySqlDataReader
            Dim mycmd As New MySql.Data.MySqlClient.MySqlCommand

            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL

            'mycmd.Parameters.AddWithValue("@email", Me.Email)

            dbRead = mycmd.ExecuteReader
            Do While dbRead.Read
                rawJson = dbRead.GetString("Opis")
            Loop
            dbRead.Close()


        End Using

        ' Retrieve JSON data from file
        'Dim rawJson = File.ReadAllText(Path.Combine(filePath, fileName))

        ' Convert to VB Class typed object
        Dim _ApiRoles As ApiRoles = JsonHelper.ToClass(Of ApiRoles)(rawJson)

        Dim dt As New DataTable
        Dim ds As New DataSet
        ds = JsonConvert.DeserializeObject(Of DataSet)(rawJson)
        dt = ds.Tables(0)
        Dim result() As DataRow = dt.Select("api_controller = '" + pController + "' AND api_method = '" + pMethod + "'")
        Dim _roles As String = result(0).Item(3)
        Dim _rolesAllowed = _roles.Split(",")


        Return _rolesAllowed

    End Function

    Public Function IsApiRoleAllowed(ByVal pController As String, ByVal pMethod As String) As Boolean

        Dim _retVal As Boolean = False
        Dim _rolesAllowed() As String

        Try
            _rolesAllowed = getApiRolesAllowed(pController, pMethod)
            _retVal = IIf(_rolesAllowed.Contains(Me.Role), True, False)

        Catch ex As Exception

        End Try

        Return _retVal

    End Function

    Public Function insApiLog(ByVal pClassName As String, ByVal pMethodName As String, ByVal pEmail As String, ByVal pRole As String, ByVal pComment As String) As Boolean

        Dim _log As String = pEmail + "-" + pRole + ": " + pClassName + "." + pMethodName

        Return insApiLog(_log, pComment)

    End Function

    Public Function insApiLog(ByVal pLog As String, ByVal pComment As String) As Boolean

        Dim ConnectionString As String = ApiGlobal.domainConnectionString
        Dim strSQL As String
        Dim _RowsAffected As Integer = -1

        strSQL = <![CDATA[
INSERT INTO sys_log (
  log_datetime, log_log, log_comment
)
VALUES
  (
   @log_datetime, @log_log, @log_comment
  ); 

    ]]>.Value

        Dim _dtNow As DateTime = DateTime.Now
        Dim _dtFormat As String = "yyyy-MM-dd HH:mm:ss"

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySql.Data.MySqlClient.MySqlCommand

            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL
            mycmd.Parameters.AddWithValue("@log_datetime", _dtNow.ToString(_dtFormat))
            mycmd.Parameters.AddWithValue("@log_log", pLog)
            mycmd.Parameters.AddWithValue("@log_comment", pComment)
            mycmd.Prepare()

            Try
                _RowsAffected = mycmd.ExecuteNonQuery()

            Catch ex As Exception
                Console.WriteLine(ex.Message)
                Return False
            End Try

        End Using



        Return If(_RowsAffected > 0, True, False)


    End Function

    Public Function ChangePassword(ByVal pEmail As String, ByVal pOldpassword As String, ByVal pNewpassword As String) As Boolean

        Dim _userName As String = Membership.GetUserNameByEmail(pEmail)
        Return Membership.Provider.ChangePassword(_userName, pOldpassword, pNewpassword)

    End Function

    Public Function CheckMinRequiredPasswordLength() As Integer

        Return Membership.Provider.MinRequiredPasswordLength

    End Function

    Public Function ResetPassword(ByVal pEmail As String, ByVal pAnswer As String) As String

        Dim _userName As String = Membership.GetUserNameByEmail(pEmail)
        Return Membership.Provider.ResetPassword(_userName, pAnswer)

    End Function

    Public Function AddRole(ByVal pRole As String, ByVal pUserEmail As String) As Boolean

        Dim _userName As String = Membership.GetUserNameByEmail(pUserEmail)

        Try
            Roles.AddUserToRole(_userName, pRole)
            Return True
        Catch ex As ArgumentNullException
            Return False
        Catch ex As ArgumentException
            Return False
        Catch ex As System.Configuration.Provider.ProviderException
            Return False
        Catch ex As Exception
            Return False
        End Try

        Return False
    End Function

    Public Function RemoveRole(ByVal pRole As String, ByVal pUserEmail As String) As Boolean

        Dim _userName As String = Membership.GetUserNameByEmail(pUserEmail)

        Try
            Roles.RemoveUserFromRole(_userName, pRole)
            Return True
        Catch ex As ArgumentNullException
            Return False
        Catch ex As ArgumentException
            Return False
        Catch ex As System.Configuration.Provider.ProviderException
            Return False
        Catch ex As Exception
            Return False
        End Try

        Return False
    End Function

    Public Function setRoles(ByVal pRoles As NgUserRoles, ByVal pUserEmail As String) As Boolean

        Dim _userName As String = Membership.GetUserNameByEmail(pUserEmail)
        Dim _userRoles As String() = Roles.GetRolesForUser(_userName)

        Dim _addRoles As New List(Of String)
        Dim _remRoles As New List(Of String)

        Try
            'Identifikuj role za remove
            '
            For Each el In _userRoles
                Select Case el
                    Case "administrator"
                        ' ima rolu 
                        If pRoles.administrator = False Then
                            ' ukida se rola
                            _remRoles.Add(el)
                        End If

                    Case "blagajna"
                        ' ima rolu 
                        If pRoles.blagajna = False Then
                            ' ukida se rola
                            _remRoles.Add(el)
                        End If

                    Case "evidencija"
                        ' ima rolu 
                        If pRoles.evidencija = False Then
                            ' ukida se rola
                            _remRoles.Add(el)
                        End If

                    Case "likvidatura"
                        ' ima rolu 
                        If pRoles.likvidatura = False Then
                            ' ukida se rola
                            _remRoles.Add(el)
                        End If

                    Case "ljudskiresursi"
                        ' ima rolu 
                        If pRoles.ljudskiresursi = False Then
                            ' ukida se rola
                            _remRoles.Add(el)
                        End If

                    Case "place"
                        ' ima rolu 
                        If pRoles.place = False Then
                            ' ukida se rola
                            _remRoles.Add(el)
                        End If

                    Case "producent"
                        ' ima rolu 
                        If pRoles.producent = False Then
                            ' ukida se rola
                            _remRoles.Add(el)
                        End If

                    Case "rukovodilac"
                        ' ima rolu 
                        If pRoles.rukovodilac = False Then
                            ' ukida se rola
                            _remRoles.Add(el)
                        End If

                    Case "sekretarica"
                        ' ima rolu 
                        If pRoles.sekretarica = False Then
                            ' ukida se rola
                            _remRoles.Add(el)
                        End If

                    Case "uposlenik"                        ' ima rolu 
                        If pRoles.uposlenik = False Then
                            ' ukida se rola
                            _remRoles.Add(el)
                        End If

                    Case "direkcija"                        ' ima rolu 
                        If pRoles.direkcija = False Then
                            ' ukida se rola
                            _remRoles.Add(el)
                        End If

                    Case "uprava"                        ' ima rolu 
                        If pRoles.uprava = False Then
                            ' ukida se rola
                            _remRoles.Add(el)
                        End If

                    Case Else
                End Select
            Next

            'Remove role
            '
            For Each el In _remRoles
                Roles.RemoveUserFromRole(_userName, el)
            Next


            'Add role
            '

            If pRoles.administrator = True AndAlso Not Roles.IsUserInRole(_userName, "administrator") Then
                ' dodaje se rola
                Roles.AddUserToRole(_userName, "administrator")
            End If


            If pRoles.blagajna = True AndAlso Not Roles.IsUserInRole(_userName, "blagajna") Then
                ' dodaje se rola
                Roles.AddUserToRole(_userName, "blagajna")
            End If


            If pRoles.evidencija = True AndAlso Not Roles.IsUserInRole(_userName, "evidencija") Then
                ' dodaje se rola
                Roles.AddUserToRole(_userName, "evidencija")
            End If


            If pRoles.likvidatura = True AndAlso Not Roles.IsUserInRole(_userName, "likvidatura") Then
                ' dodaje se rola
                Roles.AddUserToRole(_userName, "likvidatura")
            End If


            If pRoles.ljudskiresursi = True AndAlso Not Roles.IsUserInRole(_userName, "ljudskiresursi") Then
                ' dodaje se rola
                Roles.AddUserToRole(_userName, "ljudskiresursi")
            End If


            If pRoles.place = True AndAlso Not Roles.IsUserInRole(_userName, "place") Then
                ' dodaje se rola
                Roles.AddUserToRole(_userName, "place")
            End If


            If pRoles.producent = True AndAlso Not Roles.IsUserInRole(_userName, "producent") Then
                ' dodaje se rola
                Roles.AddUserToRole(_userName, "producent")
            End If


            If pRoles.rukovodilac = True AndAlso Not Roles.IsUserInRole(_userName, "rukovodilac") Then
                ' dodaje se rola
                Roles.AddUserToRole(_userName, "rukovodilac")
            End If


            If pRoles.sekretarica = True AndAlso Not Roles.IsUserInRole(_userName, "sekretarica") Then
                ' dodaje se rola
                Roles.AddUserToRole(_userName, "sekretarica")
            End If


            If pRoles.uposlenik = True AndAlso Not Roles.IsUserInRole(_userName, "uposlenik") Then
                ' dodaje se rola
                Roles.AddUserToRole(_userName, "uposlenik")
            End If

            If pRoles.direkcija = True AndAlso Not Roles.IsUserInRole(_userName, "direkcija") Then
                ' dodaje se rola
                Roles.AddUserToRole(_userName, "direkcija")
            End If


            If pRoles.uprava = True AndAlso Not Roles.IsUserInRole(_userName, "uprava") Then
                ' dodaje se rola
                Roles.AddUserToRole(_userName, "uprava")
            End If


            Return True

        Catch ex As ArgumentNullException
            Return False
        Catch ex As ArgumentException
            Return False
        Catch ex As System.Configuration.Provider.ProviderException
            Return False
        Catch ex As Exception
            Return False
        End Try

        Return False
    End Function

    Public Function getSubRole(ByVal pRole As String, ByVal pEmail As String) As String

        Dim ConnectionString As String = ApiGlobal.domainConnectionString
        Dim strSQL As String
        Dim DtList As New DataTable
        Dim subRole As String = Nothing


        strSQL = <![CDATA[ 
SELECT 
r.`name`, 
IFNULL(sr.`name`,'basic') AS `subrole`,
u2r.`userId`
FROM `my_aspnet_usersinroles` u2r
INNER JOIN `my_aspnet_roles` r ON u2r.`roleId`=r.`id`
LEFT JOIN `my_aspnet_usersinsubroles` u2sr ON u2r.`roleId`=u2sr.`roleId` AND u2r.`userId`=u2sr.`userId`
LEFT JOIN `my_aspnet_subroles` sr ON u2sr.`subroleId`=sr.`id`
LEFT JOIN `my_aspnet_membership` m ON u2r.`userId`=m.userId+1
WHERE
m.Email=@email
AND r.name=@role;
    ]]>.Value


        Using myconnection As New MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySqlCommand
            Dim mydr As MySql.Data.MySqlClient.MySqlDataReader

            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL


            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@role", pRole)
            mycmd.Parameters.AddWithValue("@email", pEmail)
            mycmd.Prepare()

            DtList.Clear()
            mydr = mycmd.ExecuteReader
            While mydr.Read
                Try
                    subRole = mydr("subrole")
                Catch ex As Exception
                End Try
            End While
            mydr.Close()

        End Using

        Return subRole

    End Function

    ' API aURL1
    ' byorg + bypg

    Public Function getEmployeeRolesList(ByVal pLstOrg As String, ByVal pLstPG As String, ByVal pTmpTableName As String,
                                         ByVal pEmployeeId As Integer) As List(Of NgUserRoles)

        Dim ConnectionString As String = ApiGlobal.domainConnectionString
        Dim strSQL, strSQLStart, strSQLOrg, strSQLPG, strSQLEnd As String
        Dim myda As MySqlClient.MySqlDataAdapter
        Dim DtList As New DataTable

        strSQL = <![CDATA[ 

        ]]>.Value

        strSQLStart = <![CDATA[ 
    DROP TABLE IF EXISTS %__tmp_employees__%;

    CREATE TEMPORARY TABLE  IF NOT EXISTS `%__tmp_employees__%` (
      `EmployeeID` INT(11) NOT NULL,
      `LastName` VARCHAR(50) CHARACTER SET cp1250 DEFAULT NULL,
      `FirstName` VARCHAR(50) CHARACTER SET cp1250 DEFAULT NULL,
      `Naziv` VARCHAR(255) CHARACTER SET cp1250 DEFAULT NULL,
      `Naziv2` VARCHAR(255) CHARACTER SET cp1250 DEFAULT NULL,
      `OrgJedSifra` VARCHAR(255) CHARACTER SET cp1250 DEFAULT NULL,
      `Email_pos` VARCHAR(45) CHARACTER SET utf8 NOT NULL DEFAULT '''',
      `Email_def` VARCHAR(45) CHARACTER SET utf8 NOT NULL DEFAULT '''',
       user_roles  VARCHAR(255) CHARACTER SET cp1250 DEFAULT NULL,
      PRIMARY KEY (`EmployeeID`)
    ) ENGINE=INNODB DEFAULT CHARSET=latin1;

        ]]>.Value

        strSQLOrg = <![CDATA[ 

    INSERT IGNORE INTO %__tmp_employees__%
    SELECT e.EmployeeID,  e.LastName, e.FirstName, 
    o.Naziv AS OrgJedNaziv, 
    o.Sifra,
    o.Sifra,
    IFNULL(ep.`custom2`, '') AS Email_pos, 
    IFNULL(ep.`custom3`, '') AS Email_def, 
    '' AS user_roles 
    FROM `evd_employees` e 
    INNER JOIN evd_employees_lpd el ON e.`EmployeeID`=el.`EmployeeID`
    INNER JOIN `sfr_organizacija` o ON e.`DepartmentUP`=o.`id` 
    LEFT JOIN `evd_employees_pim` ep ON el.`EmployeeID` = ep.`EmployeeID`
    INNER JOIN `my_aspnet_users_to_employees` u2e ON e.`EmployeeID` = u2e.`employees_id`
    WHERE 
    o.`Sifra` IN(%__ListOrgJed__%)
    AND Aktivan <> 0 
    ORDER BY e.LastName;
        ]]>.Value

        strSQLPG = <![CDATA[ 
    INSERT IGNORE INTO %__tmp_employees__%
    SELECT e.EmployeeID,  e.LastName, e.FirstName, 
    o.Naziv AS OrgJedNaziv, 
    o.Sifra,
    o.Sifra,
    IFNULL(ep.`custom2`, '') AS Email_pos, 
    IFNULL(ep.`custom3`, '') AS Email_def, 
    '' AS user_roles 
    FROM `evd_employees` e 
    INNER JOIN evd_employees_lpd el ON e.`EmployeeID`=el.`EmployeeID`
    INNER JOIN `sfr_organizacija` o ON e.`DepartmentUP`=o.`id`
     LEFT JOIN `evd_employees_pim` ep ON el.`EmployeeID` = ep.`EmployeeID`    
    INNER JOIN `my_aspnet_users_to_employees` u2e ON e.`EmployeeID` = u2e.`employees_id`
    WHERE
    e.EmployeeID IN(%__ListPGJed__%) AND Aktivan <> 0 ORDER BY e.LastName;
        ]]>.Value

        strSQLEnd = <![CDATA[ 
    UPDATE %__tmp_employees__% t, `sfr_organizacija` o
    SET 
    t.Naziv = CONCAT(t.Naziv,' (',t.OrgJedSifra,')'),
    t.Naziv2 =o.`Naziv`
    WHERE SUBSTR(t.OrgJedSifra, 1,3)=o.`Sifra`;




    UPDATE  %__tmp_employees__% AS t,	
    (SELECT a.name AS email, GROUP_CONCAT(r.name) AS user_roles FROM my_aspnet_usersinroles u
INNER JOIN my_aspnet_users a ON u.userId=a.id
INNER JOIN my_aspnet_roles r ON u.roleId=r.id
GROUP BY a.name) AS r
     SET t.user_roles=r.user_roles
     WHERE t.Email_pos = r.email;


    SELECT 
      `EmployeeID`,
      `LastName`,
      `FirstName`,
      `Naziv` AS OrgJedNaziv,
      `Naziv2` AS OrgJedNaziv2,
       OrgJedSifra,
      `Email_pos` AS Email,
      `Email_def` AS EmailDef,
       user_roles AS roles
        ,IF(INSTR(user_roles,'administrator'),1,0) AS administrator
       ,IF(INSTR(user_roles,'blagajna'),1,0) AS blagajna
       ,IF(INSTR(user_roles,'evidencija'),1,0) AS evidencija
       ,IF(INSTR(user_roles,'likvidatura'),1,0) AS likvidatura
       ,IF(INSTR(user_roles,'ljudskiresursi'),1,0) AS ljudskiresursi
       ,IF(INSTR(user_roles,'place'),1,0) AS place  
       ,IF(INSTR(user_roles,'producent'),1,0) AS producent
       ,IF(INSTR(user_roles,'rukovodilac'),1,0) AS rukovodilac
       ,IF(INSTR(user_roles,'sekretarica'),1,0) AS sekretarica
       ,IF(INSTR(user_roles,'uposlenik'),1,0) AS uposlenik     
       ,IF(INSTR(user_roles,'direkcija'),1,0) AS direkcija  
       ,IF(INSTR(user_roles,'uprava'),1,0) AS uprava  
    FROM  %__tmp_employees__% 
    ORDER BY OrgJedSifra, LastName

        ]]>.Value

        Dim rawJson As String = ""

        If pLstOrg.Length > 0 AndAlso pLstPG.Length > 0 Then
            strSQL = strSQLStart + strSQLOrg + strSQLPG + strSQLEnd
        ElseIf pLstOrg.Length > 0 AndAlso pLstPG.Length = 0 Then
            strSQL = strSQLStart + strSQLOrg + strSQLEnd
        ElseIf pLstOrg.Length = 0 AndAlso pLstPG.Length > 0 Then
            strSQL = strSQLStart + strSQLPG + strSQLEnd
        ElseIf pLstOrg.Length = 0 AndAlso pLstPG.Length = 0 Then
            strSQL = strSQLStart + strSQLEnd
        End If

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySqlCommand

            mycmd.Connection = myconnection

            strSQL = strSQL.Replace("%__tmp_employees__%", pTmpTableName)
            strSQL = strSQL.Replace("%__ListOrgJed__%", pLstOrg)
            strSQL = strSQL.Replace("%__ListPGJed__%", pLstPG)
            strSQL = strSQL.Replace("%__EmployeeId__%", pEmployeeId)

            mycmd.CommandText = strSQL

            DtList.Clear()
            myda = New MySqlDataAdapter(mycmd)
            myda.Fill(DtList)
        End Using


        Return JsonConvert.DeserializeObject(Of List(Of NgUserRoles))(GetJson(DtList))

    End Function



    ' API aUOS - spasi podatke org role
    ' byorg + bypg
    Public Function setOrgRole(ByVal pEmail As String, ByVal pOrg As String, ByVal pOrgPG As String) As Boolean

        Dim ConnectionString As String = ApiGlobal.domainConnectionString
        Dim strSQL As String
        Dim _RowsAffected As Integer = -1

        Dim _webAccessInt As Integer = 1 ' zabranjen prisup webu: IsLockedOut = 1 

        strSQL = <![CDATA[
INSERT IGNORE INTO sys_usr_role_json (
 Enabled, `Type`, Form
)
VALUES
  (
    'True', @Email, 'APIputnal'
  );


UPDATE
  sys_usr_role_json
SET
  NAME = @Name, NamePG = @NamePG, Enabled = 'Enabled'
WHERE TYPE = @Email
AND  Form = 'APIputnal';
    ]]>.Value


        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySql.Data.MySqlClient.MySqlCommand

            mycmd.Connection = myconnection
            mycmd.CommandText = strSQL



            mycmd.Parameters.AddWithValue("@Name", pOrg)
            mycmd.Parameters.AddWithValue("@NamePG", pOrgPG)
            mycmd.Parameters.AddWithValue("@Email", pEmail)
            mycmd.Prepare()

            Try
                _RowsAffected = mycmd.ExecuteNonQuery()

            Catch ex As Exception
                Console.WriteLine(ex.Message)
                Return False
            End Try

        End Using



        Return If(_RowsAffected > 0, True, False)

    End Function


    ' API aUOL1
    ' byorg + bypg

    Public Function getEmployeeOrgRolesList(ByVal pLstOrg As String, ByVal pLstPG As String, ByVal pTmpTableName As String,
                                         ByVal pEmployeeId As Integer) As List(Of NgUserOrgRoles)

        Dim ConnectionString As String = ApiGlobal.domainConnectionString
        Dim strSQL, strSQLStart, strSQLOrg, strSQLPG, strSQLEnd As String
        Dim myda As MySqlClient.MySqlDataAdapter
        Dim DtList As New DataTable

        strSQL = <![CDATA[ 

        ]]>.Value

        strSQLStart = <![CDATA[ 
    DROP TABLE IF EXISTS %__tmp_employees__%;

    CREATE TEMPORARY TABLE  IF NOT EXISTS `%__tmp_employees__%` (
      `EmployeeID` INT(11) NOT NULL,
      `LastName` VARCHAR(50) CHARACTER SET cp1250 DEFAULT NULL,
      `FirstName` VARCHAR(50) CHARACTER SET cp1250 DEFAULT NULL,
      `Naziv` VARCHAR(255) CHARACTER SET cp1250 DEFAULT NULL,
      `Naziv2` VARCHAR(255) CHARACTER SET cp1250 DEFAULT NULL,
      `OrgJedSifra` VARCHAR(255) CHARACTER SET cp1250 DEFAULT NULL,
      `Email_pos` VARCHAR(45) CHARACTER SET utf8 NOT NULL DEFAULT '''',
      `Email_def` VARCHAR(45) CHARACTER SET utf8 NOT NULL DEFAULT '''',
      OrgId  VARCHAR(255) CHARACTER SET cp1250 DEFAULT NULL,
      PGId  VARCHAR(255) CHARACTER SET cp1250 DEFAULT NULL,   
      Enabled  INT(11) NOT NULL,
      Form  VARCHAR(255) CHARACTER SET cp1250 DEFAULT NULL,  
      PRIMARY KEY (`EmployeeID`)
    ) ENGINE=INNODB DEFAULT CHARSET=latin1;

        ]]>.Value

        strSQLOrg = <![CDATA[ 

    INSERT IGNORE INTO %__tmp_employees__%
    SELECT e.EmployeeID,  e.LastName, e.FirstName, 
    o.Naziv AS OrgJedNaziv, 
    o.Sifra,
    o.Sifra,
    IFNULL(ep.`custom2`, '') AS Email_pos, 
    IFNULL(ep.`custom3`, '') AS Email_def, 
    '' AS OrgID,
    '' AS PGID,   
    0 AS Enabled,
    '' AS Form 
    FROM `evd_employees` e 
    INNER JOIN evd_employees_lpd el ON e.`EmployeeID`=el.`EmployeeID`
    INNER JOIN `sfr_organizacija` o ON e.`DepartmentUP`=o.`id` 
    LEFT JOIN `evd_employees_pim` ep ON el.`EmployeeID` = ep.`EmployeeID`
    WHERE 
    o.`Sifra` IN(%__ListOrgJed__%)
    AND Aktivan <> 0 
    ORDER BY e.LastName;
        ]]>.Value

        strSQLPG = <![CDATA[ 
    INSERT IGNORE INTO %__tmp_employees__%
    SELECT e.EmployeeID,  e.LastName, e.FirstName, 
    o.Naziv AS OrgJedNaziv, 
    o.Sifra,
    o.Sifra,
    IFNULL(ep.`custom2`, '') AS Email_pos, 
    IFNULL(ep.`custom3`, '') AS Email_def,
    '' AS OrgID,
    '' AS PGID,   
    0 AS Enabled,
    '' AS Form 
    FROM `evd_employees` e 
    INNER JOIN evd_employees_lpd el ON e.`EmployeeID`=el.`EmployeeID`
    INNER JOIN `sfr_organizacija` o ON e.`DepartmentUP`=o.`id`
     LEFT JOIN `evd_employees_pim` ep ON el.`EmployeeID` = ep.`EmployeeID`    
    WHERE
    e.EmployeeID IN(%__ListPGJed__%) AND Aktivan <> 0 ORDER BY e.LastName;
        ]]>.Value

        strSQLEnd = <![CDATA[ 
    UPDATE %__tmp_employees__% t, `sfr_organizacija` o
    SET 
    t.Naziv = CONCAT(t.Naziv,' (',t.OrgJedSifra,')'),
    t.Naziv2 =o.`Naziv`
    WHERE SUBSTR(t.OrgJedSifra, 1,3)=o.`Sifra`;

    UPDATE %__tmp_employees__% t, `evd_employees` e, `my_aspnet_users_to_employees` m 
    SET t.Form = 'isUser'
    WHERE e.`EmployeeID`=m.`employees_id`
    AND t.EmployeeID=e.EmployeeID;

    DELETE
    FROM
       %__tmp_employees__%
    WHERE LENGTH(Form) = 0;


    UPDATE %__tmp_employees__% AS t,	
	`sys_usr_role_json` r
     SET t.OrgID = r.`Name`,
         t.PGId = r.`NamePG`,
         t.Enabled = IF(r.`Enabled`='True',1,0),
         t.Form = r.Form
     WHERE t.Email_pos = r.`Type`;


    SELECT 
      `EmployeeID`
      ,`LastName`
      ,`FirstName`
      ,`Naziv` AS OrgJedNaziv
      ,`Naziv2` AS OrgJedNaziv2
      , OrgJedSifra
      ,`Email_pos` AS Email
      ,`Email_def` AS EmailDef
      ,OrgId
      ,PGId
      ,Enabled
      ,Form       
    FROM  %__tmp_employees__% 
    ORDER BY OrgJedSifra, LastName

        ]]>.Value

        Dim rawJson As String = ""

        If pLstOrg.Length > 0 AndAlso pLstPG.Length > 0 Then
            strSQL = strSQLStart + strSQLOrg + strSQLPG + strSQLEnd
        ElseIf pLstOrg.Length > 0 AndAlso pLstPG.Length = 0 Then
            strSQL = strSQLStart + strSQLOrg + strSQLEnd
        ElseIf pLstOrg.Length = 0 AndAlso pLstPG.Length > 0 Then
            strSQL = strSQLStart + strSQLPG + strSQLEnd
        ElseIf pLstOrg.Length = 0 AndAlso pLstPG.Length = 0 Then
            strSQL = strSQLStart + strSQLEnd
        End If

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySqlCommand

            mycmd.Connection = myconnection

            strSQL = strSQL.Replace("%__tmp_employees__%", pTmpTableName)
            strSQL = strSQL.Replace("%__ListOrgJed__%", pLstOrg)
            strSQL = strSQL.Replace("%__ListPGJed__%", pLstPG)
            strSQL = strSQL.Replace("%__EmployeeId__%", pEmployeeId)

            mycmd.CommandText = strSQL

            DtList.Clear()
            myda = New MySqlDataAdapter(mycmd)
            myda.Fill(DtList)
        End Using


        Return JsonConvert.DeserializeObject(Of List(Of NgUserOrgRoles))(GetJson(DtList))

    End Function


    'Public Function LockUser(ByVal pEmail As String, ByVal pAnswer As String) As String

    Public Function LockUser(ByVal pEmail As String, ByVal pApiAccess As Boolean) As Boolean

        ' zabranjen prisup webu: IsLockedOut = 1 
        ' dozvoljen prisup webu: IsLockedOut = 0
        Dim pWebAccessInt As Integer = IIf(pApiAccess = False, 0, 1)


        Dim ConnectionString As String = ApiGlobal.domainConnectionString
        Dim strSQL As String



        strSQL = <![CDATA[ 
UPDATE `my_aspnet_membership` m
SET m.`IsLockedOut` = @pWebAccess
WHERE m.`Email` = @Email;

UPDATE `my_aspnet_membership` m
SET m.LastLockedOutDate = NOW()
WHERE m.`Email` = @Email
AND m.`IsLockedOut`=1;
    ]]>.Value

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySql.Data.MySqlClient.MySqlCommand
            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL


            mycmd.Parameters.AddWithValue("@pWebAccess", pWebAccessInt)
            mycmd.Parameters.AddWithValue("@Email", pEmail)
            mycmd.Prepare()

            mycmd.ExecuteNonQuery()


        End Using

        Dim _userName As String = Membership.GetUserNameByEmail(pEmail)
        Dim user As MembershipUser = Membership.GetUser(_userName)
        Return user.IsLockedOut

    End Function

    Public Function createUser(ByVal pEmployeeID As Integer, ByVal pEmail As String) As Boolean

        Dim newUserName As String = pEmail
        Dim newUserPass As String = defUserPefix & pEmployeeID.ToString

        '
        'HACK https://docs.microsoft.com/en-us/dotnet/api/system.web.security.membershippasswordformat?redirectedfrom=MSDN&view=netframework-4.8
        '     Web.config: passwordFormat="Hashed" hashAlgorithmType="SHA256"
        '     my_aspnet_membership.PasswordFormat: 1

        Membership.CreateUser(newUserName, newUserPass, pEmail)

        Dim _userName As String = Membership.GetUserNameByEmail(pEmail)
        Dim user As MembershipUser = Membership.GetUser(_userName)

        Dim newUserId As Integer = user.ProviderUserKey

        If newUserId > -1 And newUserId <> 0 Then
            System.Web.Security.Roles.AddUserToRole(newUserName, "uposlenik")
            setUserToEmployee(newUserId, pEmployeeID)
        End If

        Return user.IsApproved

    End Function

    Public Function deleteUser(ByVal pEmail As String) As Boolean

        Dim _userName As String = Membership.GetUserNameByEmail(pEmail)
        Dim _user = Membership.Provider.GetUser(_userName, False)

        removeUserToEmployee(_user.ProviderUserKey)

        Return Membership.DeleteUser(_userName, True)

    End Function

    Public Function deleteMobUser(ByVal pEmail As String) As Boolean

        Dim _userName As String = Membership.GetUserNameByEmail(pEmail)
        Dim _user = Membership.Provider.GetUser(_userName, False)

        Return removeMobUserUUID(_user.ProviderUserKey)

    End Function

    Public Function lockUser(ByVal pEmail As String) As Boolean

        Dim ConnectionString As String = ApiGlobal.domainConnectionString
        Dim strSQL As String
        Dim _RowsAffected As Integer = -1

        Dim _webAccessInt As Integer = 1 ' zabranjen prisup webu: IsLockedOut = 1 

        strSQL = <![CDATA[
UPDATE `my_aspnet_membership` m
SET m.`IsLockedOut` = @WebAccess, 
m.LastLockedOutDate = NOW()
WHERE m.Email = @Email;
    ]]>.Value


        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySql.Data.MySqlClient.MySqlCommand

            mycmd.Connection = myconnection
            mycmd.CommandText = strSQL



            mycmd.Parameters.AddWithValue("@WebAccess", _webAccessInt)
            mycmd.Parameters.AddWithValue("@Email", pEmail)
            mycmd.Prepare()

            Try
                _RowsAffected = mycmd.ExecuteNonQuery()

            Catch ex As Exception
                Console.WriteLine(ex.Message)
                Return False
            End Try

        End Using



        Return If(_RowsAffected > 0, True, False)

    End Function

    Public Function unlockUser(ByVal pEmail As String) As Boolean

        Dim _userName As String = Membership.GetUserNameByEmail(pEmail)
        Dim _user = Membership.Provider.GetUser(_userName, False)


        Return Membership.Provider.UnlockUser(_userName)

    End Function

    Private Sub setUserToEmployee(ByVal pUserId As Integer, ByVal pEmployeeID As Integer)

        Dim ConnectionString As String = ApiGlobal.domainConnectionString

        Dim strSQL As String = <![CDATA[ 
REPLACE INTO my_aspnet_users_to_employees (users_id, employees_id)
VALUES
  (@pUserId, @pEmployeeId);
    ]]>.Value

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySql.Data.MySqlClient.MySqlCommand
            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL


            mycmd.Parameters.AddWithValue("@pUserId", pUserId)
            mycmd.Parameters.AddWithValue("@pEmployeeId", pEmployeeID)
            mycmd.Prepare()

            mycmd.ExecuteNonQuery()

        End Using
    End Sub

    Private Sub removeUserToEmployee(ByVal pUserId As Integer)

        Dim ConnectionString As String = ApiGlobal.domainConnectionString

        Dim strSQL As String = <![CDATA[ 
DELETE FROM  my_aspnet_users_to_employees
WHERE users_id = @pUserId;
    ]]>.Value

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySql.Data.MySqlClient.MySqlCommand
            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL


            mycmd.Parameters.AddWithValue("@pUserId", pUserId)
            'mycmd.Parameters.AddWithValue("@pEmployeeId", pEmployeeID)
            mycmd.Prepare()

            mycmd.ExecuteNonQuery()

        End Using
    End Sub

    Private Function removeMobUserUUID(ByVal pUserId As Integer) As Boolean

        Dim ConnectionString As String = ApiGlobal.domainConnectionString

        Dim strSQL As String = <![CDATA[ 
DELETE FROM  my_aspnet_users_to_uuid
WHERE users_id = @pUserId;
    ]]>.Value

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySql.Data.MySqlClient.MySqlCommand
            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL


            mycmd.Parameters.AddWithValue("@pUserId", pUserId)
            'mycmd.Parameters.AddWithValue("@pEmployeeId", pEmployeeID)
            mycmd.Prepare()

            Dim _RowsAffected As Integer = -1

            _RowsAffected = mycmd.ExecuteNonQuery()

            If _RowsAffected > 0 Then
                Return True
            End If

        End Using

        Return False

    End Function

    '
    'HACK isti princip se koristi u NgApiPnPutniNalog.getPnOrgPGPutniNalog
    '

    ' API L2
    ' byorg + bypg

    Public Function getEmployeeList(ByVal pLstOrg As String, ByVal pLstPG As String, ByVal pTmpTableName As String,
                                         ByVal pEmployeeId As Integer, Optional ByVal pVrsta As Integer = -1) As List(Of NgUser)

        Dim ConnectionString As String = ApiGlobal.domainConnectionString
        Dim strSQL, strSQLStart, strSQLOrg, strSQLPG, strSQLEnd As String
        Dim myda As MySqlClient.MySqlDataAdapter
        Dim DtList As New DataTable

        Dim strSQLVrsta As String = " AND Aktivan <> 0 "

        Select Case pVrsta
            Case 1
                strSQLVrsta = " AND Aktivan <> 0 "
            Case 2
                strSQLVrsta = " AND Aktivan = 0  AND Title NOT IN('S','P','K')  "
            Case 3
                strSQLVrsta = " AND Aktivan = 0 AND Title = 'S' "
            Case 4
                strSQLVrsta = " AND Aktivan = 0 AND Title = 'P' "
            Case 5
                strSQLVrsta = " AND Aktivan = 0 AND (Title = 'S' OR Title = 'P') "
            Case 6
                strSQLVrsta = " AND Aktivan = 0 "
            Case 10
                strSQLVrsta = " AND Aktivan = 0 AND Title = 'K' "
            Case Else
                strSQLVrsta = "  "
        End Select


        strSQL = <![CDATA[ 

        ]]>.Value

        strSQLStart = <![CDATA[ 
    DROP TABLE IF EXISTS %__tmp_employees__%;

    CREATE TEMPORARY TABLE  IF NOT EXISTS `%__tmp_employees__%` (
      `EmployeeID` INT(11) NOT NULL,
      `LastName` VARCHAR(50) CHARACTER SET cp1250 DEFAULT NULL,
      `FirstName` VARCHAR(50) CHARACTER SET cp1250 DEFAULT NULL,
      `Naziv` VARCHAR(255) CHARACTER SET cp1250 DEFAULT NULL,
      `Naziv2` VARCHAR(255) CHARACTER SET cp1250 DEFAULT NULL,
      `OrgJedSifra` VARCHAR(255) CHARACTER SET cp1250 DEFAULT NULL,
      `Email_pos` VARCHAR(45) CHARACTER SET utf8 NOT NULL DEFAULT '''',
      `Email_def` VARCHAR(45) CHARACTER SET utf8 NOT NULL DEFAULT '''',
      `user_status` INT(1) NOT NULL DEFAULT '0',
      `mark_status` INT(1) NOT NULL DEFAULT '0',
      `muser_status` INT(1) NOT NULL DEFAULT '0',
      PRIMARY KEY (`EmployeeID`)
    ) ENGINE=INNODB DEFAULT CHARSET=latin1;

        ]]>.Value

        strSQLOrg = <![CDATA[ 

    INSERT IGNORE INTO %__tmp_employees__%
    SELECT e.EmployeeID,  e.LastName, e.FirstName, 
    o.Naziv AS OrgJedNaziv, 
    o.Sifra,
    o.Sifra,
    IFNULL(ep.`custom2`, '') AS Email_pos, 
    IFNULL(ep.`custom3`, '') AS Email_def, 
    0 AS user_status, 0 AS mark_status,
    0 AS muser_status      
    FROM `evd_employees` e 
    INNER JOIN evd_employees_lpd el ON e.`EmployeeID`=el.`EmployeeID`
    INNER JOIN `sfr_organizacija` o ON e.`DepartmentUP`=o.`id` 
    LEFT JOIN `evd_employees_pim` ep ON el.`EmployeeID` = ep.`EmployeeID`
    WHERE 
    o.`Sifra` IN(%__ListOrgJed__%)
    %__vrsta_uposlenika__%
    ORDER BY e.LastName;
        ]]>.Value

        strSQLPG = <![CDATA[ 
    INSERT IGNORE INTO %__tmp_employees__%
    SELECT e.EmployeeID,  e.LastName, e.FirstName, 
    o.Naziv AS OrgJedNaziv, 
    o.Sifra,
    o.Sifra,
    IFNULL(ep.`custom2`, '') AS Email_pos, 
    IFNULL(ep.`custom3`, '') AS Email_def, 
    0 AS user_status, 0 AS mark_status,
    0 AS muser_status      
    FROM `evd_employees` e 
    INNER JOIN evd_employees_lpd el ON e.`EmployeeID`=el.`EmployeeID`
    INNER JOIN `sfr_organizacija` o ON e.`DepartmentUP`=o.`id`
     LEFT JOIN `evd_employees_pim` ep ON el.`EmployeeID` = ep.`EmployeeID`    
    WHERE
    e.EmployeeID IN(%__ListPGJed__%) 
    %__vrsta_uposlenika__%
    ORDER BY e.LastName;
        ]]>.Value

        strSQLEnd = <![CDATA[ 
    UPDATE %__tmp_employees__% t, `evd_employees` e, `my_aspnet_users_to_employees` m 
    SET t.user_status=-1
    WHERE e.`EmployeeID`=m.`employees_id`
    AND t.EmployeeID=e.EmployeeID;

    UPDATE %__tmp_employees__% t, `evd_employees` e, `my_aspnet_membership` m 
    SET t.mark_status=IF(m.IsLockedOut=0,-1,0)
    WHERE e.`User`=m.`userId`
    AND t.EmployeeID=e.EmployeeID;

    UPDATE %__tmp_employees__% t, `my_aspnet_membership` m, `my_aspnet_users_to_employees` m2e  
    SET t.mark_status=IF(m.IsLockedOut=0,-1,0)
    WHERE m2e.`users_id`=m.`userId`
    AND t.EmployeeID=m2e.employees_id;

    UPDATE %__tmp_employees__% t, `my_aspnet_users_to_uuid` mu, `my_aspnet_users_to_employees` m2e  
    SET t.muser_status=-1
    WHERE m2e.`users_id`=mu.`users_id`
    AND t.EmployeeID=m2e.employees_id;

        UPDATE %__tmp_employees__% t, `my_aspnet_users_to_uuid` mu, `my_aspnet_users_to_employees` m2e  
    SET t.muser_status=-1
    WHERE m2e.`users_id`=mu.`users_id`
    AND t.EmployeeID=m2e.employees_id;

    UPDATE %__tmp_employees__% t, `sfr_organizacija` o
    SET 
    t.Naziv = CONCAT(t.Naziv,' (',t.OrgJedSifra,')'),
    t.Naziv2 =o.`Naziv`
    WHERE SUBSTR(t.OrgJedSifra, 1,3)=o.`Sifra`;

    SELECT 
      `EmployeeID`,
      `LastName`,
      `FirstName`,
      `Naziv` AS OrgJedNaziv,
      `Naziv2` AS OrgJedNaziv2,
      OrgJedSifra,
      `Email_pos` AS Email,
      `Email_def` AS EmailDef,
      `user_status` AS WebUser,
      `mark_status` AS WebAccess,
      muser_status AS MobUser           
    FROM  %__tmp_employees__% 
    ORDER BY OrgJedSifra, LastName;

        ]]>.Value

        Dim rawJson As String = ""

        If pLstOrg.Length > 0 AndAlso pLstPG.Length > 0 Then
            strSQL = strSQLStart + strSQLOrg + strSQLPG + strSQLEnd
        ElseIf pLstOrg.Length > 0 AndAlso pLstPG.Length = 0 Then
            strSQL = strSQLStart + strSQLOrg + strSQLEnd
        ElseIf pLstOrg.Length = 0 AndAlso pLstPG.Length > 0 Then
            strSQL = strSQLStart + strSQLPG + strSQLEnd
        ElseIf pLstOrg.Length = 0 AndAlso pLstPG.Length = 0 Then
            strSQL = strSQLStart + strSQLEnd
        End If

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySqlCommand

            mycmd.Connection = myconnection

            strSQL = strSQL.Replace("%__tmp_employees__%", pTmpTableName)
            strSQL = strSQL.Replace("%__ListOrgJed__%", pLstOrg)
            strSQL = strSQL.Replace("%__ListPGJed__%", pLstPG)
            strSQL = strSQL.Replace("%__EmployeeId__%", pEmployeeId)
            strSQL = strSQL.Replace("%__vrsta_uposlenika__%", strSQLVrsta)

            mycmd.CommandText = strSQL

            'mycmd.Parameters.Clear()
            'mycmd.Parameters.AddWithValue("@Email", pEmail)

            DtList.Clear()
            myda = New MySqlDataAdapter(mycmd)
            myda.Fill(DtList)


        End Using


        Return JsonConvert.DeserializeObject(Of List(Of NgUser))(GetJson(DtList))

    End Function


    ' API L2
    ' byorg + bypg

    Public Function getEmployeeData(ByVal pEmployeeId As Integer) As NgUser

        Dim _tmpTable As String = "tmp_" + pEmployeeId.ToString + "_apissesion"

        Dim ConnectionString As String = ApiGlobal.domainConnectionString
        Dim strSQL, strSQLStart, strSQLOrg, strSQLEnd As String
        Dim myda As MySqlClient.MySqlDataAdapter
        Dim DtList As New DataTable

        strSQL = <![CDATA[ 

        ]]>.Value

        strSQLStart = <![CDATA[ 
    DROP TABLE IF EXISTS %__tmp_employees__%;

    CREATE TEMPORARY TABLE  IF NOT EXISTS `%__tmp_employees__%` (
      `EmployeeID` INT(11) NOT NULL,
      `LastName` VARCHAR(50) CHARACTER SET cp1250 DEFAULT NULL,
      `FirstName` VARCHAR(50) CHARACTER SET cp1250 DEFAULT NULL,
      `Naziv` VARCHAR(255) CHARACTER SET cp1250 DEFAULT NULL,
      `Naziv2` VARCHAR(255) CHARACTER SET cp1250 DEFAULT NULL,
      `OrgJedSifra` VARCHAR(255) CHARACTER SET cp1250 DEFAULT NULL,
      `Email_pos` VARCHAR(45) CHARACTER SET utf8 NOT NULL DEFAULT '''',
      `Email_def` VARCHAR(45) CHARACTER SET utf8 NOT NULL DEFAULT '''',
      `user_status` INT(1) NOT NULL DEFAULT '0',
      `mark_status` INT(1) NOT NULL DEFAULT '0',
      PRIMARY KEY (`EmployeeID`)
    ) ENGINE=INNODB DEFAULT CHARSET=latin1;

        ]]>.Value

        '
        '   Ukinuo iz Sql:  AND Aktivan <> 0 
        '
        strSQLOrg = <![CDATA[ 

    INSERT IGNORE INTO %__tmp_employees__%
    SELECT e.EmployeeID,  e.LastName, e.FirstName, 
    o.Naziv AS OrgJedNaziv, 
    o.Sifra,
    o.Sifra,
    IFNULL(ep.`custom2`, '') AS Email_pos, 
    IFNULL(ep.`custom3`, '') AS Email_def,  
    0 AS user_status, 0 AS mark_status 
    FROM `evd_employees` e 
    INNER JOIN evd_employees_lpd el ON e.`EmployeeID`=el.`EmployeeID`
    INNER JOIN `sfr_organizacija` o ON e.`DepartmentUP`=o.`id` 
    LEFT JOIN `evd_employees_pim` ep ON el.`EmployeeID` = ep.`EmployeeID`
    WHERE 
    e.EmployeeID = %__EmployeeId__%
    ORDER BY e.LastName;
        ]]>.Value


        strSQLEnd = <![CDATA[ 
    UPDATE %__tmp_employees__% t, `evd_employees` e, `my_aspnet_users_to_employees` m 
    SET t.user_status=-1
    WHERE e.`EmployeeID`=m.`employees_id`
    AND t.EmployeeID=e.EmployeeID;

    UPDATE %__tmp_employees__% t, `evd_employees` e, `my_aspnet_membership` m 
    SET t.mark_status=IF(m.IsLockedOut=0,-1,0)
    WHERE e.`User`=m.`userId`
    AND t.EmployeeID=e.EmployeeID;

    UPDATE %__tmp_employees__% t, `my_aspnet_membership` m, `my_aspnet_users_to_employees` m2e  
    SET t.mark_status=IF(m.IsLockedOut=0,-1,0)
    WHERE m2e.`users_id`=m.`userId`
    AND t.EmployeeID=m2e.employees_id;

    UPDATE %__tmp_employees__% t, `sfr_organizacija` o
    SET 
    t.Naziv = CONCAT(t.Naziv,' (',t.OrgJedSifra,')'),
    t.Naziv2 =o.`Naziv`
    WHERE SUBSTR(t.OrgJedSifra, 1,3)=o.`Sifra`;

    SELECT 
      `EmployeeID`,
      `LastName`,
      `FirstName`,
      `Naziv` AS OrgJedNaziv,
      `Naziv2` AS OrgJedNaziv2,
      OrgJedSifra,
      `Email_pos` AS Email,
      `Email_def` AS EmailDef,
      `user_status` AS WebUser,
      `mark_status` AS WebAccess     
    FROM  %__tmp_employees__% 
    ORDER BY OrgJedSifra, LastName

        ]]>.Value

        Dim rawJson As String = ""


        strSQL = strSQLStart + strSQLOrg + strSQLEnd


        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySqlCommand

            mycmd.Connection = myconnection

            strSQL = strSQL.Replace("%__tmp_employees__%", _tmpTable)
            strSQL = strSQL.Replace("%__EmployeeId__%", pEmployeeId)

            mycmd.CommandText = strSQL

            DtList.Clear()
            myda = New MySqlDataAdapter(mycmd)
            myda.Fill(DtList)
        End Using

        Try
            Dim _retVal = JsonConvert.DeserializeObject(Of List(Of NgUser))(GetJson(DtList))
            Return _retVal(0)
        Catch ex As Exception
            Console.WriteLine(ex.Message)
        End Try

        Return Nothing

    End Function


    Private Function GetJson(ByVal dt As DataTable) As String
        Dim serializer As New System.Web.Script.Serialization.JavaScriptSerializer()
        Dim rows As New List(Of Dictionary(Of String, Object))()
        Dim row As Dictionary(Of String, Object) = Nothing
        For Each dr As DataRow In dt.Rows
            row = New Dictionary(Of String, Object)()
            For Each dc As DataColumn In dt.Columns
                'If dc.ColumnName.Trim() = "TAGNAME" Then
                row.Add(dc.ColumnName.Trim(), dr(dc))
                'End If
            Next
            rows.Add(row)
        Next
        Return serializer.Serialize(rows)
    End Function

    Public Shared Function getDate(ByVal pDateString As String) As Date

        Dim format As String
        Dim result As Date
        Dim provider As CultureInfo = CultureInfo.InvariantCulture

        format = "yyyy-MM-dd"
        Try
            result = Date.ParseExact(pDateString, format, provider)
            Return result
        Catch e As FormatException

        End Try

        Return Nothing

    End Function
End Class
