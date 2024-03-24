Imports System.Data
Imports System.Web.Configuration
Imports Microsoft.VisualBasic
Imports MySql.Data
Imports Newtonsoft.Json
Imports System.Globalization

''' <summary>
''' Objekat koji predstavlja evidencije prisustva. Evidencije predstavljaju jedan kalendarski mjesec.
''' </summary>
Public Class NgApiEvidencije

    Public Sub New()

        Dim _Json As String = <![CDATA[ 
          [
        {
            itemType: 'post',
            color: 'White',
            title: 'Ukupno sati',
            sifra: 'UKP',
            sati: 0
        }
    ]
]]>.Value

        'Me.SfrEvidPris = JsonConvert.DeserializeObject(Of List(Of NgSfrEvidPris))(_Json)

        getSfrEvidPris()

    End Sub

    Public Property SfrEvidPris As List(Of NgSfrEvidPris)
    Public Property EvidPris As List(Of NgEvidDnevnnik)

    Public Property EvidPrisStatus As NgEvidPrisStatus


    Private Sub getSfrEvidPris()

        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString
        Dim strSQL As String
        Dim myda As MySqlClient.MySqlDataAdapter
        Dim DtList As New DataTable

        strSQL = <![CDATA[ 
SELECT "post" AS itemType, sp.`Color` AS color, sp.`Naziv` AS title, sp.`Sifra` AS sifra, 0 AS sati 
FROM `sfr_prisustva` sp
WHERE sp.`Sifra` NOT IN('SND');
    ]]>.Value


        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            'Dim dbRead As MySqlClient.MySqlDataReader
            Dim mycmd As New MySql.Data.MySqlClient.MySqlCommand

            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL


            DtList.Clear()
            myda = New MySqlClient.MySqlDataAdapter(strSQL, myconnection)
            myda.Fill(DtList)

            DtList.Rows.Add("post", "White", "Ukupno sati", "UKP", 0)

            Me.SfrEvidPris = JsonConvert.DeserializeObject(Of List(Of NgSfrEvidPris))(GetJson(DtList))

        End Using

    End Sub

    Public Sub getSfrEvidPris(ByVal pType As String)

        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString
        Dim strSQL As String
        Dim myda As MySqlClient.MySqlDataAdapter
        Dim DtList As New DataTable

        strSQL = <![CDATA[ 
SELECT
  id AS itemType,
  sp.`Color` AS color,
  IF(sp.`Naziv`='GO-OVAGOD','Godišnji odmor',sp.Naziv) AS title,
  IF(sp.`Sifra`='GO-OVAGOD','GO',sp.`Sifra`) AS sifra,  
  0 AS sati
FROM
  `sfr_prisustva` sp
WHERE sp.`Sifra` NOT IN (
    'SD-',
    'SD+',
    'RRP',
    'RRN',
    'PRZ',
    'GO-PROSLAGOD'
  );
    ]]>.Value


        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySql.Data.MySqlClient.MySqlCommand

            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL

            DtList.Clear()
            myda = New MySqlClient.MySqlDataAdapter(strSQL, myconnection)
            myda.Fill(DtList)

            DtList.Rows.Add("99", "White", "Redovan rad", "RRD", 0)
            Me.SfrEvidPris = JsonConvert.DeserializeObject(Of List(Of NgSfrEvidPris))(GetJson(DtList))
        End Using

    End Sub

    Public Sub getEvidPris(ByVal pEmpID As Integer, ByVal pYear As Integer, ByVal pMonth As Integer)

        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString
        Dim strSQL As String
        Dim myda As MySqlClient.MySqlDataAdapter
        Dim DtList As New DataTable

        strSQL = <![CDATA[ 
SELECT ep.`employeeID` AS empID, 
IF(LOCATE('GO-',ep.`sifra_placanja`)>0,
IF(CONVERT(SUBSTRING_INDEX(ep.`sifra_placanja`,'-',-1),UNSIGNED INTEGER)<YEAR(NOW()), 'GO-PROSLAGOD','GO-OVAGOD'),
ep.`sifra_placanja`) AS sifra,
DATE_FORMAT(ep.`datum`,'%Y-%m-%d') AS datum,
TIME_FORMAT(ep.`vrijeme_od`,'%H:%i:%s') AS vrijemeOd,
TIME_FORMAT(ep.`vrijeme_do`,'%H:%i:%s') AS vrijemeDo,
IF(IFNULL(e1.`EmployeeID`,-1)>0,CONCAT(e1.`LastName`,' ',SUBSTRING(e1.`FirstName`,1,1),'.'),'') AS Podnio,
IF(IFNULL(e2.`EmployeeID`,-1)>0,CONCAT(e2.`LastName`,' ',SUBSTRING(e2.`FirstName`,1,1),'.'),'') AS Odobrio,
IF(IFNULL(e3.`EmployeeID`,-1)>0,CONCAT(e3.`LastName`,' ',SUBSTRING(e3.`FirstName`,1,1),'.'),'') AS Kontrolisao,
IF(ep.`evd_locked`<>0,'true','false') AS Locked        
FROM `evd_prisustva` ep 
LEFT JOIN `evd_employees` e1 ON ep.`evd_podnio`=e1.`EmployeeID`
LEFT JOIN `evd_employees` e2 ON ep.`evd_odobrio`=e2.`EmployeeID`
LEFT JOIN `evd_employees` e3 ON ep.`evd_kontrolisao`=e3.`EmployeeID`
WHERE ep.`employeeID`=@empID AND YEAR(ep.`datum`)=@Year AND MONTH(ep.`datum`)=@Month
ORDER BY ep.`datum`;
    ]]>.Value



        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySql.Data.MySqlClient.MySqlCommand

            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL


            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@empID", pEmpID)
            mycmd.Parameters.AddWithValue("@Year", pYear)
            mycmd.Parameters.AddWithValue("@Month", pMonth)
            mycmd.Prepare()

            Try
                DtList.Clear()
                myda = New MySqlClient.MySqlDataAdapter(mycmd)
                myda.Fill(DtList)
            Catch ex As Exception
                Console.WriteLine(ex.Message)
            End Try


            Me.EvidPris = JsonConvert.DeserializeObject(Of List(Of NgEvidDnevnnik))(GetJson(DtList))

        End Using

    End Sub

    Public Function getEvidPris1(ByVal pEmpID As Integer, ByVal pYear As Integer, ByVal pMonth As Integer) As List(Of NgEvidDnevnik)

        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString
        Dim strSQL As String
        Dim myda As MySqlClient.MySqlDataAdapter
        Dim DtList As New DataTable

        strSQL = <![CDATA[ 
SELECT 
  ed.`evd_prisustva_id` AS id,
  ed.employeeID,
  ed.sifra_placanja,
  CONCAT(DATE_FORMAT(ed.`datum`,'%Y-%m-%d'),'T12:00:00') AS datum,
  WEEKDAY(ed.`datum`) AS week_day,
  TIME_FORMAT(ed.`vrijeme_od`,'%H:%i') AS vrijeme_od,
  TIME_FORMAT(ed.`vrijeme_do`,'%H:%i') AS vrijeme_do,
  TIME_FORMAT(ed.`vrijeme_ukupno`,'%H:%i') AS vrijeme_ukupno,
  '' AS opis_rada,
  1 AS keyday,
  IFNULL(ed.evd_podnio,0) AS evd_unio,
  ed.evd_locked AS locked,
  IFNULL(ed.evd_podnio,0) AS evd_podnio,
  IFNULL(ed.evd_odobrio,0) AS evd_odobrio,
  IFNULL(ed.evd_kontrolisao,0) AS evd_kontrolisao,
  -- IFNULL(ed.evd_stopped,0) AS evd_stopped,
  IFNULL(ed.evd_locked,0) AS locked_ext
FROM `evd_prisustva` ed 
LEFT JOIN `evd_employees` e1 ON ed.`evd_podnio`=e1.`EmployeeID`
LEFT JOIN `evd_employees` e2 ON ed.`evd_odobrio`=e2.`EmployeeID`
LEFT JOIN `evd_employees` e3 ON ed.`evd_kontrolisao`=e3.`EmployeeID`
WHERE ed.`employeeID`=@empID AND YEAR(ed.`datum`)=@Year AND MONTH(ed.`datum`)=@Month
ORDER BY ed.`datum`;
    ]]>.Value



        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySql.Data.MySqlClient.MySqlCommand

            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL


            mycmd.Parameters.AddWithValue("@empID", pEmpID)
            mycmd.Parameters.AddWithValue("@Year", pYear)
            mycmd.Parameters.AddWithValue("@Month", pMonth)
            mycmd.Prepare()

            Try
                DtList.Clear()
                myda = New MySqlClient.MySqlDataAdapter(mycmd)
                myda.Fill(DtList)
            Catch ex As Exception
                Console.WriteLine(ex.Message)
            End Try


            Return JsonConvert.DeserializeObject(Of List(Of NgEvidDnevnik))(GetJson(DtList))

        End Using

    End Function

    Public Sub getEvidPrisStatus(ByVal pEmpID As Integer, ByVal pYear As Integer, ByVal pMonth As Integer)

        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString
        Dim strSQL As String


        strSQL = <![CDATA[ 
SELECT DISTINCT ep.`employeeID` AS empID,  CONCAT(e.`FirstName`, ' ',e.`LastName`) AS empName,
IF(IFNULL(e1.`EmployeeID`,-1)>0,CONCAT(e1.`LastName`,' ',SUBSTRING(e1.`FirstName`,1,1),'.'),'') AS Podnio,
IF(IFNULL(e2.`EmployeeID`,-1)>0,CONCAT(e2.`LastName`,' ',SUBSTRING(e2.`FirstName`,1,1),'.'),'') AS Odobrio,
IF(IFNULL(e3.`EmployeeID`,-1)>0,CONCAT(e3.`LastName`,' ',SUBSTRING(e3.`FirstName`,1,1),'.'),'') AS Kontrolisao,
ep.`evd_locked` AS Locked        
FROM `evd_prisustva` ep LEFT JOIN `evd_employees` e ON ep.`employeeID`=e.`EmployeeID`
LEFT JOIN `evd_employees` e1 ON ep.`evd_podnio`=e1.`EmployeeID`
LEFT JOIN `evd_employees` e2 ON ep.`evd_odobrio`=e2.`EmployeeID`
LEFT JOIN `evd_employees` e3 ON ep.`evd_kontrolisao`=e3.`EmployeeID`
WHERE ep.`employeeID`=@empID AND YEAR(ep.`datum`)=@Year AND MONTH(ep.`datum`)=@Month
ORDER BY ep.`datum`;
    ]]>.Value



        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySql.Data.MySqlClient.MySqlCommand
            Dim dbRead As MySqlClient.MySqlDataReader

            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL


            mycmd.Parameters.AddWithValue("@empID", pEmpID)
            mycmd.Parameters.AddWithValue("@Year", pYear)
            mycmd.Parameters.AddWithValue("@Month", pMonth)
            mycmd.Prepare()

            '
            ' Provjera statusa evidencija
            '
            Me.EvidPrisStatus = New NgEvidPrisStatus

            Me.EvidPrisStatus.empID = pEmpID
            Me.EvidPrisStatus.empName = ""

            Me.EvidPrisStatus.month = pMonth
            Me.EvidPrisStatus.year = pYear

            Me.EvidPrisStatus.allowEdit = True

            Me.EvidPrisStatus.podnio = ""
            Me.EvidPrisStatus.odobrio = ""
            Me.EvidPrisStatus.kontrolisao = ""
            Me.EvidPrisStatus.locked = False

            Try
                dbRead = mycmd.ExecuteReader

                If dbRead.HasRows Then

                    Do While dbRead.Read
                        Me.EvidPrisStatus.empName = dbRead.GetString("empName")

                        If Not Me.EvidPrisStatus.podnio.Contains(dbRead.GetString("Podnio")) Then
                            Me.EvidPrisStatus.podnio += dbRead.GetString("Podnio") + ", "
                        End If

                        If Not Me.EvidPrisStatus.odobrio.Contains(dbRead.GetString("Odobrio")) Then
                            Me.EvidPrisStatus.odobrio += dbRead.GetString("Odobrio") + ", "
                        End If

                        If Not Me.EvidPrisStatus.kontrolisao.Contains(dbRead.GetString("Kontrolisao")) Then
                            Me.EvidPrisStatus.kontrolisao += dbRead.GetString("Kontrolisao") + ", "
                        End If

                        If dbRead.GetBoolean("Locked") Then
                            Me.EvidPrisStatus.locked = True
                        End If
                    Loop

                    dbRead.Close()

                    Me.EvidPrisStatus.podnio = Me.EvidPrisStatus.podnio.Trim(",", " ")
                    Me.EvidPrisStatus.odobrio = Me.EvidPrisStatus.odobrio.Trim(",", " ")
                    Me.EvidPrisStatus.kontrolisao = Me.EvidPrisStatus.kontrolisao.Trim(",", " ")

                    If Me.EvidPrisStatus.locked = True OrElse Me.EvidPrisStatus.podnio.Length > 0 OrElse Me.EvidPrisStatus.odobrio.Length > 0 OrElse Me.EvidPrisStatus.kontrolisao.Length > 0 Then
                        Me.EvidPrisStatus.allowEdit = False
                    End If

                Else
                    Me.getEmpName(pEmpID)
                    Me.EvidPrisStatus.allowEdit = True
                End If

            Catch ex As Exception
                Console.WriteLine(ex.Message)
            End Try



        End Using

    End Sub

    Public Function getEvidPrisStatus1(ByVal pEmpID As Integer, ByVal pYear As Integer, ByVal pMonth As Integer) As NgEvidDnStatus

        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString
        Dim strSQL As String


        strSQL = <![CDATA[ 
SELECT DISTINCT ep.`employeeID`,  
e.`FirstName,
e.`LastName,
ep.`evd_podnio,
ep.`evd_odobrio,
ep.evd_kontrolisao,
ep.`evd_locked` AS locked_ext        
FROM `evd_prisustva` ep 
LEFT JOIN `evd_employees` e ON ep.`employeeID`=e.`EmployeeID`
WHERE ep.`employeeID`=@empID 
AND YEAR(ep.`datum`)=@Year 
AND MONTH(ep.`datum`)=@Month;
    ]]>.Value

        Dim _EvidDnStatus = New NgEvidDnStatus

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySql.Data.MySqlClient.MySqlCommand
            Dim dbRead As MySqlClient.MySqlDataReader

            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL


            mycmd.Parameters.AddWithValue("@empID", pEmpID)
            mycmd.Parameters.AddWithValue("@Year", pYear)
            mycmd.Parameters.AddWithValue("@Month", pMonth)
            mycmd.Prepare()

            '
            ' Provjera statusa evidencija
            '
            'Dim _EvidDnStatus = New NgEvidDnStatus

            _EvidDnStatus.employeeID = pEmpID
            _EvidDnStatus.empLastName = ""
            _EvidDnStatus.empFirstName = ""

            _EvidDnStatus.MM = pMonth
            _EvidDnStatus.YYYY = pYear

            _EvidDnStatus.evd_podnio = -1
            _EvidDnStatus.evd_odobrio = -1
            _EvidDnStatus.evd_kontrolisao = -1
            _EvidDnStatus.locked_ext = False

            Try
            dbRead = mycmd.ExecuteReader

                If dbRead.HasRows Then

                    Do While dbRead.Read
                        'Me.EvidPrisStatus.empName = dbRead.GetString("empName")
                        _EvidDnStatus.empFirstName = dbRead.GetString("FirstName")
                        _EvidDnStatus.empLastName = dbRead.GetString("LastName")


                        _EvidDnStatus.evd_podnio = dbRead.GetInt32("evd_podnio")
                        _EvidDnStatus.evd_odobrio = dbRead.GetInt32("evd_odobrio")
                        _EvidDnStatus.evd_kontrolisao = dbRead.GetInt32("evd_kontrolisao")


                        If dbRead.GetBoolean("locked_ext") Then
                            _EvidDnStatus.locked_ext = True
                        End If
                    Loop

                    dbRead.Close()

                    If _EvidDnStatus.locked_ext = True OrElse _EvidDnStatus.evd_podnio > 0 OrElse _EvidDnStatus.evd_odobrio > 0 OrElse _EvidDnStatus.evd_kontrolisao > 0 Then
                        Me.EvidPrisStatus.allowEdit = False
                    End If

                Else
                    Me.getEmpName(pEmpID)
                    Me.EvidPrisStatus.allowEdit = True
                End If

            Catch ex As Exception
                Console.WriteLine(ex.Message)
            End Try


        End Using

        Return _EvidDnStatus

    End Function

    Public Function updEvidPrisPodnioStatus(ByVal pEmpID As Integer, ByVal pYear As Integer, ByVal pMonth As Integer, ByVal pPodnioEmpID As Integer) As Boolean

        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString
        Dim strSQL As String

        strSQL = <![CDATA[ 
UPDATE `evd_prisustva`
SET `evd_podnio` = @PodnioempID
WHERE YEAR(`datum`) = @Year AND MONTH(`datum`) = @Month AND `employeeID`=@empID
AND `evd_odobrio` IS NULL AND `evd_kontrolisao` IS NULL AND `evd_locked` = 0 ;
    ]]>.Value


        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySql.Data.MySqlClient.MySqlCommand

            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL


            mycmd.Parameters.AddWithValue("@empID", pEmpID)
            mycmd.Parameters.AddWithValue("@Year", pYear)
            mycmd.Parameters.AddWithValue("@Month", pMonth)
            mycmd.Prepare()

            If pPodnioEmpID <> -1 Then
                mycmd.Parameters.AddWithValue("@PodnioempID", pPodnioEmpID)
            Else
                mycmd.Parameters.AddWithValue("@PodnioempID", DBNull.Value)
            End If

            Try
                mycmd.ExecuteNonQuery()
                Return True
            Catch ex As Exception
                Console.WriteLine(ex.Message)
            End Try

        End Using

        Return False

    End Function

    Public Sub getEmpName(ByVal pEmpID As Integer)

        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString
        Dim strSQL As String


        strSQL = <![CDATA[ 
SELECT CONCAT(e.`FirstName`, ' ',e.`LastName`) AS empName
FROM `evd_employees` e 
WHERE e.`EmployeeID`=@empID;
    ]]>.Value



        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySql.Data.MySqlClient.MySqlCommand
            Dim dbRead As MySqlClient.MySqlDataReader

            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL


            mycmd.Parameters.AddWithValue("@empID", pEmpID)
            mycmd.Prepare()

            '

            Me.EvidPrisStatus.empName = ""

            Try
                dbRead = mycmd.ExecuteReader
                Do While dbRead.Read
                    Me.EvidPrisStatus.empName = dbRead.GetString("empName")
                Loop
                dbRead.Close()

            Catch ex As Exception
                Console.WriteLine(ex.Message)
            End Try



        End Using

    End Sub

    Public Sub delEvidPris(ByVal pEmpID As Integer, ByVal pYear As Integer, ByVal pMonth As Integer, ByVal pAllDates As String)

        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString
        Dim strSQL As String
        Dim DtList As New DataTable

        strSQL = <![CDATA[ 
DELETE ep.*   
FROM `evd_prisustva` ep 
WHERE ep.`employeeID`=@empID AND YEAR(ep.`datum`)=@Year AND MONTH(ep.`datum`)=@Month
AND ep.`evd_podnio` IS NULL AND ep.`evd_podnio` IS NULL AND ep.`evd_kontrolisao` IS NULL AND ep.`evd_locked` =0
AND ep.`datum` NOT IN(@AllDates);
    ]]>.Value



        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySql.Data.MySqlClient.MySqlCommand

            mycmd.Connection = myconnection



            mycmd.CommandText = strSQL.Replace("@AllDates", pAllDates)


            mycmd.Parameters.AddWithValue("@empID", pEmpID)
            mycmd.Parameters.AddWithValue("@Year", pYear)
            mycmd.Parameters.AddWithValue("@Month", pMonth)
            'mycmd.Parameters.AddWithValue("@AllDates", pAllDates)
            mycmd.Prepare()

            Try
                mycmd.ExecuteNonQuery()
            Catch ex As Exception
                Console.WriteLine(ex.Message)
            End Try


        End Using


    End Sub

    Public Sub insEvidPris(ByVal pEvidPrisTbl As List(Of NgEvidDnevnnik))


        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString
        Dim strSQL As String
        Dim DtList As New DataTable

        strSQL = <![CDATA[ 
DELETE ep.*   
FROM `evd_prisustva` ep 
WHERE ep.`employeeID`=@empID AND YEAR(ep.`datum`)=@Year AND MONTH(ep.`datum`)=@Month
AND ep.`evd_podnio` IS NULL AND ep.`evd_podnio` IS NULL AND ep.`evd_kontrolisao` IS NULL AND ep.`evd_locked` =0;
    ]]>.Value

        Dim pEmpId As Integer = -1
        Dim pYear As Integer = -1
        Dim pMonth As Integer = -1

        Dim _allDates As String = ""
        For Each el As NgEvidDnevnnik In pEvidPrisTbl

            If pEmpId = -1 AndAlso pYear = -1 AndAlso pMonth = -1 Then
                pEmpId = el.empID
                pYear = NgApiEvidencije.getDate(el.datum).Year
                pMonth = NgApiEvidencije.getDate(el.datum).Month
            Else
                If pEmpId = el.empID Then Exit For
                If pYear = NgApiEvidencije.getDate(el.datum).Year Then Exit For
                If pMonth = NgApiEvidencije.getDate(el.datum).Month Then Exit For
            End If

            _allDates += "'" + el.datum.ToString + "', "
        Next
        _allDates = _allDates.TrimEnd(",", " ")

        '
        ' Provjeri status evidencija
        '
        Me.getEvidPrisStatus(pEmpId, pYear, pMonth)
        If Me.EvidPrisStatus.allowEdit = False Then
            Me.getEvidPris(pEmpId, pYear, pMonth)
            Exit Sub
        End If

        '
        ' Obriši redove kojih nema u dolaznoj tabeli
        '
        Me.delEvidPris(pEmpId, pYear, pMonth, _allDates)

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySql.Data.MySqlClient.MySqlCommand

            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL


            mycmd.Parameters.AddWithValue("@empID", pEmpId)
            mycmd.Parameters.AddWithValue("@Year", pYear)
            mycmd.Parameters.AddWithValue("@Month", pMonth)
            mycmd.Prepare()

            Try
                mycmd.ExecuteNonQuery()
            Catch ex As Exception
                Console.WriteLine(ex.Message)
            End Try


            strSQL = <![CDATA[ 
INSERT INTO `evd_prisustva` 
 (`employeeID`, `sifra_placanja`, `datum`, `vrijeme_od`, `vrijeme_do`, `vrijeme_ukupno` )
VALUES
  (@employeeID, @sifra_placanja, @datum, @vrijeme_od, @vrijeme_do, @vrijeme_ukupno );
    ]]>.Value

            mycmd.CommandText = strSQL


            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@employeeID", -1)
            mycmd.Parameters.AddWithValue("@sifra_placanja", "Dummy")
            mycmd.Parameters.AddWithValue("@datum", "1900-01-01")
            mycmd.Parameters.AddWithValue("@vrijeme_od", "08:00:00")
            mycmd.Parameters.AddWithValue("@vrijeme_do", "16:00:00")
            mycmd.Parameters.AddWithValue("@vrijeme_ukupno", "08:00:00")
            mycmd.Prepare()

            Dim _curYear As Integer = Date.Now.Year

            Try
                For Each el As NgEvidDnevnnik In pEvidPrisTbl
                    mycmd.Parameters("@employeeID").Value = el.empID

                    Select Case el.sifra
                        Case "GO-OVAGOD"
                            mycmd.Parameters("@sifra_placanja").Value = "GO-" + _curYear.ToString
                        Case "GO-PROSLAGOD"
                            mycmd.Parameters("@sifra_placanja").Value = "GO-" + (_curYear - 1).ToString
                        Case Else
                            mycmd.Parameters("@sifra_placanja").Value = el.sifra
                    End Select

                    'mycmd.Parameters("@sifra_placanja").Value = el.sifra
                    mycmd.Parameters("@datum").Value = el.datum
                    mycmd.Parameters("@vrijeme_od").Value = el.vrijemeOd
                    mycmd.Parameters("@vrijeme_do").Value = el.vrijemeDo
                    mycmd.Parameters("@vrijeme_ukupno").Value = el.vrijemeOd

                    mycmd.ExecuteNonQuery()
                Next

            Catch ex As Exception
                Console.WriteLine(ex.Message)
            End Try

        End Using

        Me.getEvidPris(pEmpId, pYear, pMonth)

    End Sub

    Public Function insEvidPris1(ByVal pEvidPrisTbl As List(Of NgEvidDnevnik)) As List(Of NgEvidDnevnik)


        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString
        Dim strSQL As String
        Dim DtList As New DataTable

        strSQL = <![CDATA[ 
DELETE ep.*   
FROM `evd_prisustva` ep 
WHERE ep.`employeeID`=@empID AND YEAR(ep.`datum`)=@Year AND MONTH(ep.`datum`)=@Month
AND ep.`evd_podnio` IS NULL AND ep.`evd_podnio` IS NULL AND ep.`evd_kontrolisao` IS NULL AND ep.`evd_locked` =0;
    ]]>.Value

        Dim pEmpId As Integer = -1
        Dim pYear As Integer = -1
        Dim pMonth As Integer = -1

        Dim _allDates As String = ""
        For Each el As NgEvidDnevnik In pEvidPrisTbl

            If pEmpId = -1 AndAlso pYear = -1 AndAlso pMonth = -1 Then
                pEmpId = el.employeeID
                pYear = NgApiEvidencije.getDate(el.datum).Year
                pMonth = NgApiEvidencije.getDate(el.datum).Month
            Else
                If pEmpId = el.employeeID Then Exit For
                If pYear = NgApiEvidencije.getDate(el.datum).Year Then Exit For
                If pMonth = NgApiEvidencije.getDate(el.datum).Month Then Exit For
            End If

            _allDates += "'" + el.datum.ToString + "', "
        Next
        _allDates = _allDates.TrimEnd(",", " ")

        '
        ' Provjeri status evidencija
        '
        Me.getEvidPrisStatus(pEmpId, pYear, pMonth)
        If Me.EvidPrisStatus.allowEdit = False Then
            Return Me.getEvidPris1(pEmpId, pYear, pMonth)
        End If

        '
        ' Obriši redove kojih nema u dolaznoj tabeli
        '
        Me.delEvidPris(pEmpId, pYear, pMonth, _allDates)

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySql.Data.MySqlClient.MySqlCommand

            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL


            mycmd.Parameters.AddWithValue("@empID", pEmpId)
            mycmd.Parameters.AddWithValue("@Year", pYear)
            mycmd.Parameters.AddWithValue("@Month", pMonth)
            mycmd.Prepare()

            Try
                mycmd.ExecuteNonQuery()
            Catch ex As Exception
                Console.WriteLine(ex.Message)
            End Try


            strSQL = <![CDATA[ 
INSERT INTO `evd_prisustva` 
 (`employeeID`, `sifra_placanja`, `datum`, `vrijeme_od`, `vrijeme_do`, `vrijeme_ukupno` )
VALUES
  (@employeeID, @sifra_placanja, @datum, @vrijeme_od, @vrijeme_do, @vrijeme_ukupno );
    ]]>.Value

            mycmd.CommandText = strSQL


            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@employeeID", -1)
            mycmd.Parameters.AddWithValue("@sifra_placanja", "Dummy")
            mycmd.Parameters.AddWithValue("@datum", "1900-01-01")
            mycmd.Parameters.AddWithValue("@vrijeme_od", "08:00:00")
            mycmd.Parameters.AddWithValue("@vrijeme_do", "16:00:00")
            mycmd.Parameters.AddWithValue("@vrijeme_ukupno", "08:00:00")
            mycmd.Prepare()

            Dim _curYear As Integer = Date.Now.Year

            Try
                For Each el As NgEvidDnevnik In pEvidPrisTbl
                    mycmd.Parameters("@employeeID").Value = el.employeeID

                    Select Case el.sifra_placanja
                        Case "GO-OVAGOD"
                            mycmd.Parameters("@sifra_placanja").Value = "GO-" + _curYear.ToString
                        Case "GO-PROSLAGOD"
                            mycmd.Parameters("@sifra_placanja").Value = "GO-" + (_curYear - 1).ToString
                        Case Else
                            mycmd.Parameters("@sifra_placanja").Value = el.sifra_placanja
                    End Select

                    'mycmd.Parameters("@sifra_placanja").Value = el.sifra
                    mycmd.Parameters("@datum").Value = el.datum
                    mycmd.Parameters("@vrijeme_od").Value = el.vrijeme_od
                    mycmd.Parameters("@vrijeme_do").Value = el.vrijeme_do
                    mycmd.Parameters("@vrijeme_ukupno").Value = el.vrijeme_od

                    mycmd.ExecuteNonQuery()
                Next

            Catch ex As Exception
                Console.WriteLine(ex.Message)
            End Try

        End Using

        Return Me.getEvidPris1(pEmpId, pYear, pMonth)

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
