Imports System.Data
Imports System.Web.Configuration
Imports Microsoft.VisualBasic
Imports MySql.Data
Imports MySql.Data.MySqlClient
Imports Newtonsoft.Json
Imports System.Globalization
Imports System.Security.Cryptography

Public Class NgApiEmployee

    Public Sub New()

    End Sub

    Public Property List As List(Of NgEmployee)

    ' API US-L1
    ' 
    Public Sub listItems(ByVal pStatus As String)

        'Dim ConnectionString As String = ApiGlobal.domainConnectionString
        Dim ConnectionString As String = ApiGlobal.domainConnectionString

        Dim strSQL, strSQLEnd1, strSQLEnd2 As String
        Dim myda As MySqlClient.MySqlDataAdapter
        Dim DtList As New DataTable

        strSQL = <![CDATA[ 
SELECT
e.EmployeeID,
e.EmployeeNumber,
e.Title,
e.JMB,
e.FirstName,
e.MiddleName,
e.LastName,
e.BrSocOsig,
e.Opc_Stanovanja,
e.Djeca,
e.RadnoMjesto,
e.PostalCode,
e.DepartmentID,
e.OfficeLocation,
 DATE_FORMAT(e.Dat_Rodjenja,'%Y-%m-%d') AS Dat_Rodjenja,
 DATE_FORMAT(e.Dat_Zaposl,'%Y-%m-%d') AS Dat_Zaposl,
 DATE_FORMAT(e.Dat_Odlaska,'%Y-%m-%d') AS Dat_Odlaska,
e.Aktivan,
e.Razlog_Odlaska,
e.DepartmentUP,
e.Pol,
el.ImeRoditelja,
el.Broj_LK,
el.Mjesto_LK,
el.Opcina_LK,
el.Drzavljanstvo,
el.Narod,
el.BracnoStanje,
ep.emp_hm_telephone,
ep.emp_hm_mobile,
ep.emp_oth_email,
ep.emp_work_extphone,
ep.emp_work_telephone,
ep.emp_work_mobile,
ep.emp_work_email,
ep.emp_work_default_email 
FROM evd_employees e
INNER JOIN evd_employees_lpd el
  ON e.EmployeeID = el.EmployeeID
LEFT JOIN evd_employees_pim ep
  ON e.EmployeeID = ep.EmployeeID
    ]]>.Value

        strSQLEnd1 = <![CDATA[ 
WHERE
e.Aktivan <> 0;
    ]]>.Value

        strSQLEnd2 = <![CDATA[ 
WHERE
e.Aktivan = 0
AND e.Title=@status;
    ]]>.Value

        Select Case pStatus
            Case "A"
                strSQL = strSQL + strSQLEnd1
            Case "", "S", "P", "K"
                strSQL = strSQL + strSQLEnd2
        End Select

        Using myconnection As New MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySqlCommand
            mycmd.Connection = myconnection

            'strSQL = strSQL.Replace("%__Days__%", pDays)
            mycmd.CommandText = strSQL



            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@status", pStatus)
            mycmd.Prepare()

            DtList.Clear()
            myda = New MySqlDataAdapter(mycmd)
            myda.Fill(DtList)

            Me.List = JsonConvert.DeserializeObject(Of List(Of NgEmployee))(ngApiGeneral.GetJson(DtList))
        End Using

    End Sub

    Public Function listItems(ByVal pIds As List(Of Integer), Optional ByVal pStatus As String = "X") As List(Of NgEmployee)

        'Dim ConnectionString As String = ApiGlobal.domainConnectionString
        Dim ConnectionString As String = ApiGlobal.domainConnectionString

        Dim strSQL, strSQLEnd As String
        Dim myda As MySqlClient.MySqlDataAdapter
        Dim DtList As New DataTable

        strSQL = <![CDATA[ 
SELECT
e.EmployeeID,
e.EmployeeNumber,
e.Title,
e.JMB,
e.FirstName,
e.MiddleName,
e.LastName,
e.BrSocOsig,
e.Opc_Stanovanja,
e.Djeca,
e.RadnoMjesto,
e.PostalCode,
e.DepartmentID,
e.OfficeLocation,
 DATE_FORMAT(e.Dat_Rodjenja,'%Y-%m-%d') AS Dat_Rodjenja,
 DATE_FORMAT(e.Dat_Zaposl,'%Y-%m-%d') AS Dat_Zaposl,
 DATE_FORMAT(e.Dat_Odlaska,'%Y-%m-%d') AS Dat_Odlaska,
e.Aktivan,
e.Razlog_Odlaska,
e.DepartmentUP,
e.Pol,
el.ImeRoditelja,
el.Broj_LK,
el.Mjesto_LK,
el.Opcina_LK,
el.Drzavljanstvo,
el.Narod,
el.BracnoStanje,
ep.emp_hm_telephone,
ep.emp_hm_mobile,
ep.emp_oth_email,
ep.emp_work_extphone,
ep.emp_work_telephone,
ep.emp_work_mobile,
ep.emp_work_email,
ep.emp_work_default_email,
ep.`custom2`  AS emp_username
FROM evd_employees e
INNER JOIN evd_employees_lpd el
  ON e.EmployeeID = el.EmployeeID
LEFT JOIN evd_employees_pim ep
  ON e.EmployeeID = ep.EmployeeID
    ]]>.Value




        Select Case pStatus
            Case "A"
                strSQLEnd = <![CDATA[ 
WHERE
e.Aktivan <> 0 AND e.EmployeeID IN(@EmployeeID);
    ]]>.Value
                strSQL = strSQL + strSQLEnd

            Case "", "S", "P", "K"
                strSQLEnd = <![CDATA[ 
WHERE
e.Aktivan = 0
AND e.Title=@status AND e.EmployeeID IN(@EmployeeID);
    ]]>.Value
                strSQL = strSQL + strSQLEnd

            Case "X"
                strSQLEnd = <![CDATA[ 
WHERE
 e.EmployeeID IN(@EmployeeID);
    ]]>.Value
                strSQL = strSQL + strSQLEnd
        End Select

        Using myconnection As New MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySqlCommand

            mycmd.Connection = myconnection

            Dim _ids As String = String.Join(",", pIds)
            strSQL = strSQL.Replace("@EmployeeID", _ids)
            mycmd.CommandText = strSQL


            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@status", pStatus)
            'mycmd.Parameters.AddWithValue("@EmployeeID", _ids)
            mycmd.Prepare()

            DtList.Clear()
            myda = New MySqlDataAdapter(mycmd)

            myda.Fill(DtList)

            'DtList.Rows.Add("post", "White", "Ukupno sati", "UKP", 0)

            Me.List = JsonConvert.DeserializeObject(Of List(Of NgEmployee))(ngApiGeneral.GetJson(DtList))

        End Using



        Return List

    End Function

    ' API US-L2
    ' 
    Public Function listItem(ByVal pId As Integer, Optional ByVal pStatus As String = "X") As NgEmployee

        Dim ConnectionString As String = ApiGlobal.domainConnectionString
        Dim strSQL, strSQLEnd As String
        Dim myda As MySqlClient.MySqlDataAdapter
        Dim DtList As New DataTable

        strSQL = <![CDATA[ 
SELECT
e.EmployeeID,
e.EmployeeNumber,
e.Title,
e.JMB,
e.FirstName,
e.MiddleName,
e.LastName,
e.BrSocOsig,
e.Opc_Stanovanja,
e.Djeca,
e.RadnoMjesto,
e.PostalCode,
e.DepartmentID,
e.OfficeLocation,
 DATE_FORMAT(e.Dat_Rodjenja,'%Y-%m-%d') AS Dat_Rodjenja,
 DATE_FORMAT(e.Dat_Zaposl,'%Y-%m-%d') AS Dat_Zaposl,
 DATE_FORMAT(e.Dat_Odlaska,'%Y-%m-%d') AS Dat_Odlaska,
e.Aktivan,
e.Razlog_Odlaska,
e.DepartmentUP,
e.Pol,
el.ImeRoditelja,
el.Broj_LK,
el.Mjesto_LK,
el.Opcina_LK,
el.Drzavljanstvo,
el.Narod,
el.BracnoStanje,
ep.emp_hm_telephone,
ep.emp_hm_mobile,
ep.emp_oth_email,
ep.emp_work_extphone,
ep.emp_work_telephone,
ep.emp_work_mobile,
ep.emp_work_email,
ep.emp_work_default_email,
ep.`custom2`  AS emp_username
FROM evd_employees e
INNER JOIN evd_employees_lpd el
  ON e.EmployeeID = el.EmployeeID
LEFT JOIN evd_employees_pim ep
  ON e.EmployeeID = ep.EmployeeID
    ]]>.Value




        Select Case pStatus
            Case "A"
                strSQLEnd = <![CDATA[ 
WHERE
e.Aktivan <> 0 AND e.EmployeeID=@EmployeeID;
    ]]>.Value
                strSQL = strSQL + strSQLEnd

            Case "", "S", "P", "K"
                strSQLEnd = <![CDATA[ 
WHERE
e.Aktivan = 0
AND e.Title=@status AND e.EmployeeID=@EmployeeID;
    ]]>.Value
                strSQL = strSQL + strSQLEnd

            Case "X"
                strSQLEnd = <![CDATA[ 
WHERE
 e.EmployeeID=@EmployeeID;
    ]]>.Value
                strSQL = strSQL + strSQLEnd
        End Select

        Using myconnection As New MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySqlCommand

            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL


            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@status", pStatus)
            mycmd.Parameters.AddWithValue("@EmployeeID", pId)
            mycmd.Prepare()

            DtList.Clear()
            myda = New MySqlDataAdapter(mycmd)

            myda.Fill(DtList)

            'DtList.Rows.Add("post", "White", "Ukupno sati", "UKP", 0)

            Me.List = JsonConvert.DeserializeObject(Of List(Of NgEmployee))(ngApiGeneral.GetJson(DtList))

        End Using

        If List.Count = 0 Then

            Dim _empEployee = New NgEmployee()
            _empEployee.EmployeeID = -1
            _empEployee.FirstName = ""
            _empEployee.LastName = ""
            List.Insert(0, _empEployee)
        End If


        Return List.Item(0)

    End Function

    ' API US-L3
    ' 
    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="pLstOrg"></param>
    ''' <param name="pLstPG"></param>
    ''' <param name="pTmpTableName"></param>
    ''' <param name="pEmployeeId"></param>
    ''' <param name="pVrsta"></param>
    ''' <returns></returns>
    Public Function listItems(ByVal pLstOrg As String, ByVal pLstPG As String, ByVal pTmpTableName As String,
                                         ByVal pEmployeeId As Integer, Optional ByVal pVrsta As Integer = -1) As List(Of NgEmployee)

        Dim ConnectionString As String = ApiGlobal.domainConnectionString
        Dim strSQL, strSQLStart, strSQLOrg, strSQLPG, strSQLEnd As String
        Dim strSQLFix As String
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

        strSQLFix = <![CDATA[ 
INSERT INTO `evd_employees_pim` (`EmployeeID`)
SELECT e.`EmployeeID`
FROM  `evd_employees` e LEFT JOIN `evd_employees_pim` ep ON e.`EmployeeID`=ep.`EmployeeID`
WHERE ep.`EmployeeID` IS NULL;

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
  `emp_work_extphone` VARCHAR (50) CHARACTER SET utf8 DEFAULT NULL,
  `emp_work_telephone` VARCHAR (50) CHARACTER SET utf8 DEFAULT NULL,
  `emp_work_mobile` VARCHAR (50) CHARACTER SET utf8 DEFAULT NULL,
  `emp_work_email` VARCHAR (50) CHARACTER SET utf8 DEFAULT NULL,
  `emp_hm_telephone` VARCHAR (50) CHARACTER SET utf8 DEFAULT NULL,
  `emp_hm_mobile` VARCHAR (50) CHARACTER SET utf8 DEFAULT NULL,
  `emp_oth_email` VARCHAR (50) CHARACTER SET utf8 DEFAULT NULL,
  `emp_work_default_email` INT (1) DEFAULT '0',
  `emp_username` VARCHAR (250) CHARACTER SET utf8 NOT NULL DEFAULT '', 
  `work_station` INT (1) DEFAULT '0',
      PRIMARY KEY (`EmployeeID`)
    ) ENGINE=INNODB DEFAULT CHARSET=cp1250;

        ]]>.Value

        strSQLOrg = <![CDATA[ 

    INSERT IGNORE INTO %__tmp_employees__%
    SELECT e.EmployeeID,  
    e.LastName, 
    e.FirstName, 
    o.Naziv AS OrgJedNaziv, 
    o.Sifra,
    o.Sifra,
    ep.`emp_work_extphone`,
    ep.`emp_work_telephone`,
    ep.`emp_work_mobile`,
    ep.`emp_work_email`,
    ep.`emp_hm_telephone`,
    ep.`emp_hm_mobile`,
    ep.`emp_oth_email`,
    ep.`emp_work_default_email`,
    IFNULL(ep.`custom2`, '') AS emp_username,
    ep.`work_station`             
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
    ep.`emp_work_extphone`,
    ep.`emp_work_telephone`,
    ep.`emp_work_mobile`,
    ep.`emp_work_email`,
    ep.`emp_hm_telephone`,
    ep.`emp_hm_mobile`,
    ep.`emp_oth_email`,
    ep.`emp_work_default_email`,
    IFNULL(ep.`custom2`, '') AS emp_username,
    ep.`work_station`          
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
  `OrgJedSifra`,
  `emp_work_extphone`,
  `emp_work_telephone`,
  `emp_work_mobile`,
  `emp_work_email`,
  `emp_hm_telephone`,
  `emp_hm_mobile`,
  `emp_oth_email`,
  `emp_work_default_email`,
  `emp_username`,
  IFNULL(`work_station`,0) AS work_station  
FROM
    %__tmp_employees__% 
ORDER BY OrgJedSifra, 
LastName;

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

            strSQL = strSQLFix + strSQL

            mycmd.CommandText = strSQL


            DtList.Clear()
            myda = New MySqlDataAdapter(mycmd)
            myda.Fill(DtList)
        End Using


        Me.List = JsonConvert.DeserializeObject(Of List(Of NgEmployee))(ngApiGeneral.GetJson(DtList))

        Return Me.List

    End Function

    Public Function listItems(ByVal pVar As String, ByVal pLstOrg As String, ByVal pLstPG As String, ByVal pTmpTableName As String,
                                         ByVal pEmployeeId As Integer, Optional ByVal pVrsta As Integer = -1) As List(Of NgEmployee)

        Select Case pVar
            Case "email_exclude"
                Return listItems_Email(pLstOrg, pLstPG, pTmpTableName, pEmployeeId, pVrsta)
            Case Else
                Return listItems(pLstOrg, pLstPG, pTmpTableName, pEmployeeId, pVrsta)
        End Select

    End Function

    Public Function listItems_Email(ByVal pLstOrg As String, ByVal pLstPG As String, ByVal pTmpTableName As String,
                                         ByVal pEmployeeId As Integer, Optional ByVal pVrsta As Integer = -1) As List(Of NgEmployee)

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
  `emp_work_extphone` VARCHAR (50) CHARACTER SET utf8 DEFAULT NULL,
  `emp_work_telephone` VARCHAR (50) CHARACTER SET utf8 DEFAULT NULL,
  `emp_work_mobile` VARCHAR (50) CHARACTER SET utf8 DEFAULT NULL,
  `emp_work_email` VARCHAR (50) CHARACTER SET utf8 DEFAULT NULL,
  `emp_hm_telephone` VARCHAR (50) CHARACTER SET utf8 DEFAULT NULL,
  `emp_hm_mobile` VARCHAR (50) CHARACTER SET utf8 DEFAULT NULL,
  `emp_oth_email` VARCHAR (50) CHARACTER SET utf8 DEFAULT NULL,
  `emp_work_default_email` INT (1) DEFAULT '0',
  `emp_username` VARCHAR (250) CHARACTER SET utf8 NOT NULL DEFAULT '', 
  `work_station` INT (1) DEFAULT '0',
  `emp_email_exclude` VARCHAR (1024) CHARACTER SET utf8 NOT NULL DEFAULT '', 
      PRIMARY KEY (`EmployeeID`)
    ) ENGINE=INNODB DEFAULT CHARSET=cp1250;

        ]]>.Value

        strSQLOrg = <![CDATA[ 

    INSERT IGNORE INTO %__tmp_employees__%
    SELECT e.EmployeeID,  
    e.LastName, 
    e.FirstName, 
    o.Naziv AS OrgJedNaziv, 
    o.Sifra,
    o.Sifra,
    ep.`emp_work_extphone`,
    ep.`emp_work_telephone`,
    ep.`emp_work_mobile`,
    ep.`emp_work_email`,
    ep.`emp_hm_telephone`,
    ep.`emp_hm_mobile`,
    ep.`emp_oth_email`,
    ep.`emp_work_default_email`,
    IFNULL(ep.`custom2`, '') AS emp_username,
    ep.`work_station`,
    '' AS `emp_email_exclude`                 
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
    ep.`emp_work_extphone`,
    ep.`emp_work_telephone`,
    ep.`emp_work_mobile`,
    ep.`emp_work_email`,
    ep.`emp_hm_telephone`,
    ep.`emp_hm_mobile`,
    ep.`emp_oth_email`,
    ep.`emp_work_default_email`,
    IFNULL(ep.`custom2`, '') AS emp_username,
    ep.`work_station`,
    '' AS `emp_email_exclude`              
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
    UPDATE %__tmp_employees__% t, `notify_exclude` ne
    SET 
    t.`emp_email_exclude`  = ne.`template_name`
    WHERE t.`EmployeeID` = ne.`EmployeeID`;

SELECT
  `EmployeeID`,
  `LastName`,
  `FirstName`,
  `Naziv` AS OrgJedNaziv,
  `Naziv2` AS OrgJedNaziv2,
  `OrgJedSifra`,
  `emp_work_extphone`,
  `emp_work_telephone`,
  `emp_work_mobile`,
  `emp_work_email`,
  `emp_hm_telephone`,
  `emp_hm_mobile`,
  `emp_oth_email`,
  `emp_work_default_email`,
  `emp_username`,
  IFNULL(`work_station`,0) AS work_station,
  `emp_email_exclude`   
FROM
    %__tmp_employees__% 
ORDER BY OrgJedSifra, 
LastName;

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

            DtList.Clear()
            myda = New MySqlDataAdapter(mycmd)
            myda.Fill(DtList)
        End Using


        Me.List = JsonConvert.DeserializeObject(Of List(Of NgEmployee))(ngApiGeneral.GetJson(DtList))

        Return Me.List

    End Function









    ' API US-C1
    ' 
    Public Function insItem(ByRef pInsItem As NgEmployee) As Integer

        Dim ConnectionString As String = ApiGlobal.domainConnectionString
        Dim strSQL As String
        Dim _RowsAffected As Integer = -1
        Dim _LastInsertedId As Integer = -1

        strSQL = <![CDATA[
INSERT INTO evd_employees (
    Title,
    JMB,
    FirstName,
    MiddleName,
    LastName,
    Opc_Stanovanja
)
VALUES
  (
    @Title,
    @JMB,
    @FirstName,
    @MiddleName,
    @LastName,
    @Opc_Stanovanja
  );
    ]]>.Value


        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySql.Data.MySqlClient.MySqlCommand

            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL


            mycmd.Parameters.Clear()
            'mycmd.Parameters.AddWithValue("@EmployeeNumber", pInsItem.EmployeeNumber)
            mycmd.Parameters.AddWithValue("@Title", pInsItem.Title)
            mycmd.Parameters.AddWithValue("@JMB", pInsItem.JMB)
            mycmd.Parameters.AddWithValue("@FirstName", pInsItem.FirstName)
            mycmd.Parameters.AddWithValue("@MiddleName", pInsItem.MiddleName)
            mycmd.Parameters.AddWithValue("@LastName", pInsItem.LastName)
            'mycmd.Parameters.AddWithValue("@BrSocOsig", pInsItem.BrSocOsig)
            mycmd.Parameters.AddWithValue("@Opc_Stanovanja", pInsItem.Opc_Stanovanja)
            'mycmd.Parameters.AddWithValue("@Djeca", pInsItem.Djeca)
            'mycmd.Parameters.AddWithValue("@RadnoMjesto", pInsItem.RadnoMjesto)
            'mycmd.Parameters.AddWithValue("@PostalCode", pInsItem.PostalCode)
            'mycmd.Parameters.AddWithValue("@DepartmentID", pInsItem.DepartmentID)
            'mycmd.Parameters.AddWithValue("@DepartmentUP", pInsItem.DepartmentUP)
            'mycmd.Parameters.AddWithValue("@OfficeLocation", pInsItem.OfficeLocation)
            'mycmd.Parameters.AddWithValue("@Dat_Rodjenja", pInsItem.Dat_Rodjenja)
            'mycmd.Parameters.AddWithValue("@Dat_Zaposl", pInsItem.Dat_Zaposl)
            'mycmd.Parameters.AddWithValue("@Dat_Odlaska", pInsItem.Dat_Odlaska)
            'mycmd.Parameters.AddWithValue("@Aktivan", pInsItem.Aktivan)
            'mycmd.Parameters.AddWithValue("@Razlog_Odlaska", pInsItem.Razlog_Odlaska)
            'mycmd.Parameters.AddWithValue("@Pol", pInsItem.Pol)
            mycmd.Prepare()

            Try
                _RowsAffected = mycmd.ExecuteNonQuery()

                If _RowsAffected > 0 Then
                    _LastInsertedId = mycmd.LastInsertedId
                    pInsItem.EmployeeID = _LastInsertedId

                    insItemA(pInsItem)
                    insItemB(pInsItem)

                End If

            Catch ex As Exception
                Console.WriteLine(ex.Message)
            End Try

        End Using

        Return IIf(_RowsAffected > 0, _LastInsertedId, -1)

    End Function

    Private Function insItemA(ByRef pInsItem As NgEmployee) As Integer

        Dim ConnectionString As String = ApiGlobal.domainConnectionString
        Dim strSQL As String
        Dim _RowsAffected As Integer = -1

        strSQL = <![CDATA[
INSERT INTO evd_employees_lpd (
  EmployeeID,
  JMB
)
VALUES
  (
  @EmployeeID,
  @JMB
  );
    ]]>.Value


        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySql.Data.MySqlClient.MySqlCommand

            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL


            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@EmployeeID", pInsItem.EmployeeID)
            mycmd.Parameters.AddWithValue("@JMB", pInsItem.JMB)
            'mycmd.Parameters.AddWithValue("@ImeRoditelja", pInsItem.ImeRoditelja)
            'mycmd.Parameters.AddWithValue("@Broj_LK", pInsItem.Broj_LK)
            'mycmd.Parameters.AddWithValue("@Mjesto_LK", pInsItem.Mjesto_LK)
            'mycmd.Parameters.AddWithValue("@Opcina_LK", pInsItem.Opcina_LK)
            'mycmd.Parameters.AddWithValue("@Drzavljanstvo", pInsItem.Drzavljanstvo)
            'mycmd.Parameters.AddWithValue("@Narod", pInsItem.Narod)
            'mycmd.Parameters.AddWithValue("@BracnoStanje", pInsItem.BracnoStanje)
            mycmd.Prepare()

            Try
                _RowsAffected = mycmd.ExecuteNonQuery()


            Catch ex As Exception
                Console.WriteLine(ex.Message)
            End Try

        End Using

        Return IIf(_RowsAffected > 0, _RowsAffected, -1)

    End Function

    Private Function insItemB(ByRef pInsItem As NgEmployee) As Integer

        Dim ConnectionString As String = ApiGlobal.domainConnectionString
        Dim strSQL As String
        Dim _RowsAffected As Integer = -1

        strSQL = <![CDATA[
INSERT INTO evd_employees_pim (
  EmployeeID
)
VALUES
  (
  @EmployeeID
  );
    ]]>.Value


        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySql.Data.MySqlClient.MySqlCommand

            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL


            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@EmployeeID", pInsItem.EmployeeID)
            'mycmd.Parameters.AddWithValue("@emp_hm_telephone", pInsItem.emp_hm_telephone)
            'mycmd.Parameters.AddWithValue("@emp_hm_mobile", pInsItem.emp_hm_mobile)
            'mycmd.Parameters.AddWithValue("@emp_oth_email", pInsItem.emp_oth_email)
            'mycmd.Parameters.AddWithValue("@emp_work_extphone", pInsItem.emp_work_extphone)
            'mycmd.Parameters.AddWithValue("@emp_work_telephone", pInsItem.emp_work_telephone)
            'mycmd.Parameters.AddWithValue("@emp_work_mobile", pInsItem.emp_work_mobile)
            'mycmd.Parameters.AddWithValue("@emp_work_email", pInsItem.emp_work_email)
            'mycmd.Parameters.AddWithValue("@emp_work_default_email", pInsItem.emp_work_default_email)
            mycmd.Prepare()

            Try
                _RowsAffected = mycmd.ExecuteNonQuery()


            Catch ex As Exception
                Console.WriteLine(ex.Message)
            End Try

        End Using

        Return IIf(_RowsAffected > 0, _RowsAffected, -1)

    End Function

    ' API US-S1
    ' 
    Public Function updItem(ByRef pInsItem As NgEmployee) As Boolean

        Dim ConnectionString As String = ApiGlobal.domainConnectionString
        Dim strSQL As String
        Dim _RowsAffected As Integer = -1

        strSQL = <![CDATA[
UPDATE
  evd_employees
SET
    EmployeeNumber = @EmployeeNumber,
    Title = @Title,
    JMB=  @JMB,
    FirstName = @FirstName,
    MiddleName = @MiddleName,
    LastName = @LastName,
    BrSocOsig = @BrSocOsig,
    Opc_Stanovanja = @Opc_Stanovanja,
    Djeca = @Djeca,
    RadnoMjesto = @RadnoMjesto,
    PostalCode = @PostalCode,
    DepartmentID = @DepartmentID,
    DepartmentUP = @DepartmentUP,
    OfficeLocation = @OfficeLocation, 
    Dat_Rodjenja = @Dat_Rodjenja,
    Dat_Zaposl = @Dat_Zaposl,
    Dat_Odlaska = @Dat_Odlaska,
    Aktivan = @Aktivan,
    Razlog_Odlaska = @Razlog_Odlaska,
    Pol = @Pol
WHERE EmployeeID = @EmployeeID;
    ]]>.Value


        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySql.Data.MySqlClient.MySqlCommand

            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL


            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@EmployeeID", pInsItem.EmployeeID)
            mycmd.Parameters.AddWithValue("@EmployeeNumber", pInsItem.EmployeeNumber)
            mycmd.Parameters.AddWithValue("@Title", pInsItem.Title)
            mycmd.Parameters.AddWithValue("@JMB", pInsItem.JMB)
            mycmd.Parameters.AddWithValue("@FirstName", pInsItem.FirstName)
            mycmd.Parameters.AddWithValue("@MiddleName", pInsItem.MiddleName)
            mycmd.Parameters.AddWithValue("@LastName", pInsItem.LastName)
            mycmd.Parameters.AddWithValue("@BrSocOsig", pInsItem.BrSocOsig)
            mycmd.Parameters.AddWithValue("@Opc_Stanovanja", pInsItem.Opc_Stanovanja)
            mycmd.Parameters.AddWithValue("@Djeca", pInsItem.Djeca)
            mycmd.Parameters.AddWithValue("@RadnoMjesto", pInsItem.RadnoMjesto)
            mycmd.Parameters.AddWithValue("@PostalCode", pInsItem.PostalCode)
            mycmd.Parameters.AddWithValue("@DepartmentID", pInsItem.DepartmentID)
            mycmd.Parameters.AddWithValue("@DepartmentUP", pInsItem.DepartmentUP)
            mycmd.Parameters.AddWithValue("@OfficeLocation", pInsItem.OfficeLocation)
            mycmd.Parameters.AddWithValue("@Dat_Rodjenja", pInsItem.Dat_Rodjenja)
            mycmd.Parameters.AddWithValue("@Dat_Zaposl", pInsItem.Dat_Zaposl)
            mycmd.Parameters.AddWithValue("@Dat_Odlaska", pInsItem.Dat_Odlaska)
            mycmd.Parameters.AddWithValue("@Aktivan", pInsItem.Aktivan)
            mycmd.Parameters.AddWithValue("@Razlog_Odlaska", pInsItem.Razlog_Odlaska)
            mycmd.Parameters.AddWithValue("@Pol", pInsItem.Pol)
            mycmd.Prepare()

            Try
                _RowsAffected = mycmd.ExecuteNonQuery()

                updItemA(pInsItem)
                updItemB(pInsItem)

            Catch ex As Exception
                Console.WriteLine(ex.Message)
            End Try

        End Using

        Return IIf(_RowsAffected > 0, True, False)

    End Function




    Public Function updItemA(ByRef pInsItem As NgEmployee) As Boolean

        Dim ConnectionString As String = ApiGlobal.domainConnectionString
        Dim strSQL As String
        Dim _RowsAffected As Integer = -1

        strSQL = <![CDATA[
UPDATE
  evd_employees_lpd
SET
  JMB = @JMB,
  ImeRoditelja =  @ImeRoditelja,
  Broj_LK = @Broj_LK,
  Mjesto_LK = @Mjesto_LK,
  Opcina_LK = @Opcina_LK,
  Drzavljanstvo = @Drzavljanstvo,
  Narod = @Narod,
  BracnoStanje = @BracnoStanje
WHERE EmployeeID = @EmployeeID;
    ]]>.Value


        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySql.Data.MySqlClient.MySqlCommand

            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL


            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@EmployeeID", pInsItem.EmployeeID)
            mycmd.Parameters.AddWithValue("@JMB", pInsItem.JMB)
            mycmd.Parameters.AddWithValue("@ImeRoditelja", pInsItem.ImeRoditelja)
            mycmd.Parameters.AddWithValue("@Broj_LK", pInsItem.Broj_LK)
            mycmd.Parameters.AddWithValue("@Mjesto_LK", pInsItem.Mjesto_LK)
            mycmd.Parameters.AddWithValue("@Opcina_LK", pInsItem.Opcina_LK)
            mycmd.Parameters.AddWithValue("@Drzavljanstvo", pInsItem.Drzavljanstvo)
            mycmd.Parameters.AddWithValue("@Narod", pInsItem.Narod)
            mycmd.Parameters.AddWithValue("@BracnoStanje", pInsItem.BracnoStanje)
            mycmd.Prepare()

            Try
                _RowsAffected = mycmd.ExecuteNonQuery()
            Catch ex As Exception
                Console.WriteLine(ex.Message)
            End Try

        End Using

        Return IIf(_RowsAffected > 0, True, False)

    End Function

    ''' <summary>
    ''' Update PIM kontakt podatke
    ''' </summary>
    ''' <param name="pInsItem"></param>
    ''' <returns></returns>
    Public Function updItemB(ByRef pInsItem As NgEmployee) As Boolean

        Dim ConnectionString As String = ApiGlobal.domainConnectionString
        Dim strSQL As String
        Dim _RowsAffected As Integer = -1

        strSQL = <![CDATA[
UPDATE
  evd_employees_pim
SET
  emp_hm_telephone = @emp_hm_telephone,
  emp_hm_mobile = @emp_hm_mobile,
  emp_oth_email = @emp_oth_email,
  emp_work_extphone = @emp_work_extphone,
  emp_work_telephone = @emp_work_telephone,
  emp_work_mobile = @emp_work_mobile,
  emp_work_email = @emp_work_email,
  emp_work_default_email = @emp_work_default_email
WHERE EmployeeID = @EmployeeID;
    ]]>.Value


        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySql.Data.MySqlClient.MySqlCommand

            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL


            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@EmployeeID", pInsItem.EmployeeID)
            mycmd.Parameters.AddWithValue("@emp_hm_telephone", pInsItem.emp_hm_telephone)
            mycmd.Parameters.AddWithValue("@emp_hm_mobile", pInsItem.emp_hm_mobile)
            mycmd.Parameters.AddWithValue("@emp_oth_email", pInsItem.emp_oth_email)
            mycmd.Parameters.AddWithValue("@emp_work_extphone", pInsItem.emp_work_extphone)
            mycmd.Parameters.AddWithValue("@emp_work_telephone", pInsItem.emp_work_telephone)
            mycmd.Parameters.AddWithValue("@emp_work_mobile", pInsItem.emp_work_mobile)
            mycmd.Parameters.AddWithValue("@emp_work_email", pInsItem.emp_work_email)
            mycmd.Parameters.AddWithValue("@emp_work_default_email", pInsItem.emp_work_default_email)
            mycmd.Prepare()

            Try
                _RowsAffected = mycmd.ExecuteNonQuery()
            Catch ex As Exception
                Console.WriteLine(ex.Message)
            End Try

        End Using

        Return IIf(_RowsAffected > 0, True, False)

    End Function

    Public Function updItemB1(ByRef pInsItem As NgEmployee) As Boolean

        Dim ConnectionString As String = ApiGlobal.domainConnectionString
        Dim strSQL As String
        Dim _RowsAffected As Integer = -1

        strSQL = <![CDATA[
UPDATE `evd_employees` e LEFT JOIN `evd_employees_pim` ep ON e.`EmployeeID`=ep.`EmployeeID`
SET ep.`custom2`=ep.`emp_work_email`,
ep.`custom3`=ep.`emp_work_default_email`
WHERE e.EmployeeID = @EmployeeID
AND ep.`custom2` IS NULL
AND ep.`emp_work_default_email`=1;
    ]]>.Value


        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySql.Data.MySqlClient.MySqlCommand

            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL


            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@EmployeeID", pInsItem.EmployeeID)
            mycmd.Prepare()

            Try
                _RowsAffected = mycmd.ExecuteNonQuery()
            Catch ex As Exception
                Console.WriteLine(ex.Message)
            End Try

        End Using

        Return IIf(_RowsAffected > 0, True, False)

    End Function

    Public Function updItemByField(ByRef pInsItem As NgEmployee, ByVal pFieldName As String) As Boolean

        Dim ConnectionString As String = ApiGlobal.domainConnectionString
        Dim strSQL As String
        Dim _RowsAffected As Integer = -1

        strSQL = <![CDATA[
    ]]>.Value

        Select Case pFieldName
            Case "work_station"
                strSQL = <![CDATA[
UPDATE
  evd_employees_pim
SET
  work_station = @work_station
WHERE EmployeeID = @EmployeeID;
    ]]>.Value

            Case "emp_email_exclude"
                strSQL = <![CDATA[
REPLACE INTO `notify_exclude` (
  `EmployeeID`,
  `template_name`
)
VALUES
  (
    @EmployeeID,
    @emp_email_exclude
  );
    ]]>.Value
        End Select

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySql.Data.MySqlClient.MySqlCommand

            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL


            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@EmployeeID", pInsItem.EmployeeID)
            mycmd.Prepare()

            Select Case pFieldName
                Case "work_station"
                    mycmd.Parameters.AddWithValue("@work_station", pInsItem.work_station)
                Case "emp_email_exclude"
                    mycmd.Parameters.AddWithValue("@emp_email_exclude", pInsItem.emp_email_exclude)
            End Select


            Try
                _RowsAffected = mycmd.ExecuteNonQuery()
            Catch ex As Exception
                Console.WriteLine(ex.Message)
            End Try

        End Using

        Return IIf(_RowsAffected > 0, True, False)

    End Function

    ' API A-D1
    ' 
    Public Function delItem(ByRef pDelItem As NgEmployee) As Boolean

        Dim ConnectionString As String = ApiGlobal.domainConnectionString
        Dim strSQL As String
        Dim _RowsAffected As Integer = -1

        strSQL = <![CDATA[
DELETE
FROM
 evd_employees
WHERE EmployeeID = @EmployeeID;

DELETE
FROM
 evd_employees_lpd
WHERE EmployeeID = @EmployeeID;

DELETE
FROM
 evd_employees_pim
WHERE EmployeeID = @EmployeeID;
    ]]>.Value




        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySql.Data.MySqlClient.MySqlCommand

            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL


            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@EmployeeID", pDelItem.EmployeeID)
            mycmd.Prepare()

            Try
                _RowsAffected = mycmd.ExecuteNonQuery()
            Catch ex As Exception
                Console.WriteLine(ex.Message)
            End Try

        End Using

        Return IIf(_RowsAffected > 0, True, False)

    End Function

    Public Function listItemOrgTree() As String


        Return ""
    End Function

    Public Function listItemPG() As String


        Return ""
    End Function

    Public Function listItems_PG(ByVal pIdPG As String) As List(Of Integer)

        Dim ConnectionString As String = ApiGlobal.domainConnectionString
        Dim strSQL As String
        Dim DtList As New DataTable

        strSQL = <![CDATA[ 
 SELECT pg.`Opis`
 FROM `sfr_poslovnegrupe` pg
 WHERE pg.`Sifra`=@pIdPG;
    ]]>.Value

        Dim _IdPG As String = ""

        Using myconnection As New MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySqlCommand

            mycmd.Connection = myconnection
            mycmd.CommandText = strSQL


            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@pIdPG", pIdPG)
            mycmd.Prepare()

            Dim myReader As MySqlDataReader
            myReader = mycmd.ExecuteReader()
            Try
                While myReader.Read()
                    _IdPG = myReader.GetString(0)
                End While
            Finally
                myReader.Close()
            End Try

        End Using

        Dim sIdPG() As String
        sIdPG = _IdPG.Split(","c)

        Dim lIdPg As New List(Of Integer)

        For Each el In sIdPG
            lIdPg.Add(CInt(el))
        Next

        Return lIdPg
    End Function

End Class
