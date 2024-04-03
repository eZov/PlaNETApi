Imports System.Data
Imports System.Web.Configuration
Imports Microsoft.VisualBasic
Imports MySql.Data
Imports Newtonsoft.Json
Imports System.Globalization
Imports MySql.Data.MySqlClient

Public Class NgApiEvidDnevnik

    Public Sub New()

    End Sub

    Public Sub New(ByVal pEmployeeID As Integer, ByVal pMM As Integer, ByVal pYYYY As Integer)
        Me.EmployeeID = pEmployeeID
        Me.MM = pMM
        Me.YYYY = pYYYY

        Me.EvidDnevnik = New List(Of NgEvidDnevnik)
    End Sub

    Public Sub New(ByVal pMM As Integer, ByVal pYYYY As Integer)
        Me.MM = pMM
        Me.YYYY = pYYYY
    End Sub

    Public Property EmployeeID As Integer
    Public Property MM As Integer
    Public Property YYYY As Integer

    Public Property EvdPodnio As Integer = 0
    Public Property EvdOdobrio As Integer = 0
    Public Property EvdKontrolisao As Integer = 0
    Public Property EvdStopped As Integer = 0
    Public Property EvdlockedExt As Boolean = False

    Public Property EvidDnevnik As List(Of NgEvidDnevnik)

    Public Sub getEvidDnevnikExt()

        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString
        Dim strSQL As String
        Dim DtList As New DataTable

        strSQL = <![CDATA[ 
SELECT 
  IFNULL(edx.evd_podnio,0) AS evd_podnio,
  IFNULL(edx.evd_odobrio,0) AS evd_odobrio,
  IFNULL(edx.evd_kontrolisao,0) AS evd_kontrolisao,
  IFNULL(edx.evd_stopped,0) AS evd_stopped,
  IFNULL(edx.evd_locked,0) AS evd_locked_ext
FROM evd_scheddnevnik_ext edx 
WHERE
edx.employeeID = @employeeID
AND edx.`mjesec` = @MM
AND edx.`godina` = @YYYY;
    ]]>.Value

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySql.Data.MySqlClient.MySqlCommand

            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL


            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@employeeID", Me.EmployeeID)
            mycmd.Parameters.AddWithValue("@MM", Me.MM)
            mycmd.Parameters.AddWithValue("@YYYY", Me.YYYY)
            mycmd.Prepare()

            Try
                DtList.Clear()
                Dim rd As MySqlDataReader = mycmd.ExecuteReader()

                While rd.Read
                    EvdPodnio = rd.GetInt16(rd.GetOrdinal("evd_podnio"))
                    EvdOdobrio = rd.GetInt16(rd.GetOrdinal("evd_odobrio"))
                    EvdKontrolisao = rd.GetInt16(rd.GetOrdinal("evd_kontrolisao"))
                    EvdStopped = rd.GetInt16(rd.GetOrdinal("evd_stopped"))
                    EvdlockedExt = rd.GetBoolean(rd.GetOrdinal("evd_locked_ext"))
                End While
            Catch ex As Exception

            End Try

        End Using

    End Sub

    Public Sub getEvidDnevnikDummy()

        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString
        Dim strSQL As String
        Dim DtList As New DataTable

        strSQL = <![CDATA[ 
CALL EVDwb_create_tmp_dummy(@employeeID, @MM, @YYYY);
    ]]>.Value

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySql.Data.MySqlClient.MySqlCommand
            Dim myda As MySqlClient.MySqlDataAdapter

            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL


            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@employeeID", Me.EmployeeID)
            mycmd.Parameters.AddWithValue("@MM", Me.MM)
            mycmd.Parameters.AddWithValue("@YYYY", Me.YYYY)
            mycmd.Prepare()

            Try
                DtList.Clear()
                myda = New MySqlClient.MySqlDataAdapter(mycmd)
                myda.Fill(DtList)

                Me.EvidDnevnik = JsonConvert.DeserializeObject(Of List(Of NgEvidDnevnik))(ngApiGeneral.GetJson(DtList))
            Catch ex As Exception

            End Try

        End Using

    End Sub

    Public Sub getEvidDnevnik()

        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString
        Dim strSQL As String
        Dim DtList As New DataTable

        strSQL = <![CDATA[ 
SELECT 
  ed.id,
  ed.employeeID,
  ed.sifra_placanja,
  CONCAT(DATE_FORMAT(ed.`datum`,'%Y-%m-%d'),'T12:00:00') AS datum,
  WEEKDAY(ed.`datum`) AS week_day,
  TIME_FORMAT(ed.`vrijeme_od`,'%H:%i') AS vrijeme_od,
  TIME_FORMAT(ed.`vrijeme_do`,'%H:%i') AS vrijeme_do,
  TIME_FORMAT(ed.`vrijeme_ukupno`,'%H:%i') AS vrijeme_ukupno,
  ed.opis_rada,
  ed.keyday,
  IFNULL(ed.evd_podnio,0) AS evd_unio,
  ed.evd_locked AS locked,
  IFNULL(edx.evd_podnio,0) AS evd_podnio,
  IFNULL(edx.evd_odobrio,0) AS evd_odobrio,
  IFNULL(edx.evd_kontrolisao,0) AS evd_kontrolisao,
  IFNULL(edx.evd_stopped,0) AS evd_stopped,
  IFNULL(edx.evd_locked,0) AS locked_ext
FROM evd_scheddnevnik ed
LEFT JOIN evd_scheddnevnik_ext edx ON ed.employeeID=edx.employeeID 
AND YEAR(ed.datum)=edx.godina AND MONTH(ed.datum)=edx.mjesec
WHERE
ed.employeeID = @employeeID
AND MONTH(ed.datum) = @MM
AND YEAR(ed.datum) = @YYYY
ORDER BY ed.`datum`, ed.`vrijeme_od`;
    ]]>.Value

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySql.Data.MySqlClient.MySqlCommand
            Dim myda As MySqlClient.MySqlDataAdapter

            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL


            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@employeeID", Me.EmployeeID)
            mycmd.Parameters.AddWithValue("@MM", Me.MM)
            mycmd.Parameters.AddWithValue("@YYYY", Me.YYYY)
            mycmd.Prepare()

            Try
                DtList.Clear()
                myda = New MySqlClient.MySqlDataAdapter(mycmd)
                myda.Fill(DtList)

                Me.EvidDnevnik = JsonConvert.DeserializeObject(Of List(Of NgEvidDnevnik))(ngApiGeneral.GetJson(DtList))
            Catch ex As Exception

            End Try

        End Using

    End Sub

    Public Function getEvidDnevnik(ByVal pDataTable As Boolean) As DataTable

        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString
        Dim strSQL As String
        Dim DtList As New DataTable

        strSQL = <![CDATA[ 
SELECT 
  ed.id,
  ed.employeeID,
  ed.sifra_placanja,
  CONCAT(DATE_FORMAT(ed.`datum`,'%Y-%m-%d'),'T12:00:00') AS datum,
  WEEKDAY(ed.`datum`) AS week_day,
  TIME_FORMAT(ed.`vrijeme_od`,'%H:%i') AS vrijeme_od,
  TIME_FORMAT(ed.`vrijeme_do`,'%H:%i') AS vrijeme_do,
  TIME_FORMAT(ed.`vrijeme_ukupno`,'%H:%i') AS vrijeme_ukupno,
  ed.opis_rada,
  ed.keyday,
  IFNULL(ed.evd_podnio,0) AS evd_unio,
  ed.evd_locked AS locked,
  IFNULL(edx.evd_podnio,0) AS evd_podnio,
  IFNULL(edx.evd_odobrio,0) AS evd_odobrio,
  IFNULL(edx.evd_kontrolisao,0) AS evd_kontrolisao,
  IFNULL(edx.evd_locked,0) AS locked_ext
FROM evd_scheddnevnik ed
LEFT JOIN evd_scheddnevnik_ext edx ON ed.employeeID=edx.employeeID 
AND YEAR(ed.datum)=edx.godina AND MONTH(ed.datum)=edx.mjesec
WHERE
ed.employeeID = @employeeID
AND MONTH(ed.datum) = @MM
AND YEAR(ed.datum) = @YYYY
ORDER BY ed.`datum`, ed.`vrijeme_od`;
    ]]>.Value

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySql.Data.MySqlClient.MySqlCommand
            Dim myda As MySqlClient.MySqlDataAdapter

            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL


            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@employeeID", Me.EmployeeID)
            mycmd.Parameters.AddWithValue("@MM", Me.MM)
            mycmd.Parameters.AddWithValue("@YYYY", Me.YYYY)
            mycmd.Prepare()

            Try
                DtList.Clear()
                myda = New MySqlClient.MySqlDataAdapter(mycmd)
                myda.Fill(DtList)

                Me.EvidDnevnik = JsonConvert.DeserializeObject(Of List(Of NgEvidDnevnik))(ngApiGeneral.GetJson(DtList))
            Catch ex As Exception

            End Try

        End Using

        Return DtList

    End Function

    Public Sub insEvidDnevnik(ByVal pEmpId As Integer, Optional ByVal pPreparedDnevnik As Boolean = True)

        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString
        Dim strSQL As String
        Dim DtList As New DataTable

        strSQL = <![CDATA[ 
INSERT INTO evd_scheddnevnik (
  employeeID,
  datum,
  vrijeme_od,
  vrijeme_do,
  sifra_placanja,
  keyday,
  evd_podnio,
  evd_locked
)
VALUES
  (
    @employeeID,
    @datum,
    @vrijeme_od,
    @vrijeme_do,
    @sifra_placanja,
    @keyday,
    @evd_podnio,
    @evd_locked
  );
    ]]>.Value

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySql.Data.MySqlClient.MySqlCommand

            mycmd.Connection = myconnection

            Dim dt As New DateTime(Me.YYYY, Me.MM, 1)
            Dim format As String = "yyyy-MM-dd"
            Dim _sfrPlacanja As String = ""
            Dim _vrijemeOd As String = "08:00"
            Dim _vrijemeDo As String = "16:00"

            Try
                While dt.Month = Me.MM
                    mycmd.CommandText = strSQL
        

                    Select Case pPreparedDnevnik
                        '
                        ' Kreiraj mjesec sa popunjenim radom 8/5 (40 sati sedmično)
                        Case True
                            If dt.DayOfWeek() = DayOfWeek.Saturday Or dt.DayOfWeek() = DayOfWeek.Sunday Then
                                _sfrPlacanja = "SND"
                                _vrijemeOd = ""
                                _vrijemeDo = ""
                            Else
                                _sfrPlacanja = "RRD"
                                _vrijemeOd = "08:00"
                                _vrijemeDo = "16:00"
                            End If

                         '
                         ' Kreiraj prazan mjesec sa slobodnim danima bez sati
                        Case False
                            _sfrPlacanja = "SND"
                            _vrijemeOd = ""
                            _vrijemeDo = ""

                    End Select



                    mycmd.Parameters.Clear()
                    mycmd.Parameters.AddWithValue("@employeeID", Me.EmployeeID)
                    mycmd.Parameters.AddWithValue("@datum", dt.ToString(format))
                    mycmd.Parameters.AddWithValue("@sifra_placanja", _sfrPlacanja)
                    mycmd.Parameters.AddWithValue("@vrijeme_od", _vrijemeOd)
                    mycmd.Parameters.AddWithValue("@vrijeme_do", _vrijemeDo)
                    mycmd.Parameters.AddWithValue("@keyday", True)
                    mycmd.Parameters.AddWithValue("@evd_podnio", pEmpId)
                    mycmd.Parameters.AddWithValue("@evd_locked", False)
                    mycmd.Prepare()

                    mycmd.ExecuteNonQuery()
                    dt = dt.AddDays(1)
                End While

                strSQL = <![CDATA[ 
INSERT INTO evd_scheddnevnik_ext (
  employeeID,
  mjesec,
  godina,
  evd_podnio,
  evd_odobrio,
  evd_kontrolisao,
  evd_stopped,
  evd_locked
)
VALUES
  (
    @employeeID,
    @mjesec,
    @godina,
    0,
    0,
    0,
    0,
    0
  );
    ]]>.Value

                mycmd.CommandText = strSQL
    

                mycmd.Parameters.Clear()
                mycmd.Parameters.AddWithValue("@employeeID", Me.EmployeeID)
                mycmd.Parameters.AddWithValue("@mjesec", Me.MM)
                mycmd.Parameters.AddWithValue("@godina", Me.YYYY)
                mycmd.Prepare()

                mycmd.ExecuteNonQuery()
            Catch ex As Exception

                Console.WriteLine(ex.Message)
            End Try



        End Using

    End Sub

    Public Sub insEvidDnevnikRow(ByVal pEmpId As Integer, ByRef pEvidDnevnik As List(Of NgEvidDnevnik))

        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString
        Dim strSQL As String
        Dim DtList As New DataTable

        strSQL = <![CDATA[ 
INSERT INTO evd_scheddnevnik (
  employeeID,
  datum,
  vrijeme_od,
  vrijeme_do,
  sifra_placanja,
  opis_rada,
  keyday,
  evd_podnio,
  evd_locked
)
VALUES
  (
    @employeeID,
    @datum,
    @vrijeme_od,
    @vrijeme_do,
    @sifra_placanja,
    @opis_rada,
    @keyday,
    @evd_podnio,
    @evd_locked
  );
    ]]>.Value

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySql.Data.MySqlClient.MySqlCommand

            mycmd.Connection = myconnection

            Try
                For Each el In pEvidDnevnik
                    '
                    ' Preskoči key red - ili locked red
                    '
                    If el.keyday = True Or el.locked = True Then
                        Continue For
                    End If

                    mycmd.CommandText = strSQL
        

                    mycmd.Parameters.Clear()
                    mycmd.Parameters.AddWithValue("@employeeID", el.employeeID)
                    mycmd.Parameters.AddWithValue("@datum", el.datum)
                    mycmd.Parameters.AddWithValue("@sifra_placanja", el.sifra_placanja)
                    mycmd.Parameters.AddWithValue("@opis_rada", el.opis_rada)
                    mycmd.Parameters.AddWithValue("@vrijeme_od", el.vrijeme_od)
                    mycmd.Parameters.AddWithValue("@vrijeme_do", el.vrijeme_do)
                    mycmd.Parameters.AddWithValue("@keyday", False)
                    mycmd.Parameters.AddWithValue("@evd_podnio", pEmpId)
                    mycmd.Parameters.AddWithValue("@evd_locked", False)
                    mycmd.Prepare()

                    mycmd.ExecuteNonQuery()
                Next

            Catch ex As Exception

                Console.WriteLine(ex.Message)
            End Try



        End Using

    End Sub

    Public Sub delEvidDnevnikRow(ByRef pEvidDnevnik As List(Of NgEvidDnevnik))

        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString
        Dim strSQL As String
        Dim DtList As New DataTable

        strSQL = <![CDATA[ 
DELETE
FROM
  evd_scheddnevnik
WHERE id = @id;
    ]]>.Value

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySql.Data.MySqlClient.MySqlCommand

            mycmd.Connection = myconnection

            Try
                For Each el In pEvidDnevnik
                    '
                    ' Preskoči key red - ili locked red
                    '
                    If el.keyday = True Or el.locked = True Then
                        Continue For
                    End If

                    mycmd.CommandText = strSQL
        

                    mycmd.Parameters.Clear()
                    mycmd.Parameters.AddWithValue("@id", el.id)
                    mycmd.Prepare()

                    mycmd.ExecuteNonQuery()
                Next

            Catch ex As Exception

                Console.WriteLine(ex.Message)
            End Try



        End Using

    End Sub

    Public Sub updEvidDnevnikRow(ByVal pEmpId As Integer, ByRef pEvidDnevnik As List(Of NgEvidDnevnik), Optional ByVal pSuperLock As Boolean = False)

        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString
        Dim strSQL As String
        Dim DtList As New DataTable

        strSQL = <![CDATA[ 
UPDATE
  evd_scheddnevnik
SET
  sifra_placanja = @sifra_placanja,
  vrijeme_od = @vrijeme_od,
  vrijeme_do = @vrijeme_do,
  opis_rada = @opis_rada,
  evd_podnio = @evd_podnio,
  evd_locked = @evd_locked
WHERE id = @id;
    ]]>.Value

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySql.Data.MySqlClient.MySqlCommand

            mycmd.Connection = myconnection

            Try
                For Each el In pEvidDnevnik

                    Dim _evdLock As Integer = 0

                    ' evidenciej unosi UPOSLENIK
                    If el.locked = True AndAlso pSuperLock = False Then
                        '
                        ' Preskoči locked red
                        Continue For
                    ElseIf pSuperLock = True Then
                        '
                        ' Ovaj red je superlocked, upiši 2
                        _evdLock = IIf(el.locked = True, 2, 0)
                    End If



                    mycmd.CommandText = strSQL
        

                    mycmd.Parameters.Clear()
                    mycmd.Parameters.AddWithValue("@id", el.id)
                    mycmd.Parameters.AddWithValue("@sifra_placanja", el.sifra_placanja)
                    mycmd.Parameters.AddWithValue("@opis_rada", el.opis_rada)
                    mycmd.Parameters.AddWithValue("@vrijeme_od", el.vrijeme_od)
                    mycmd.Parameters.AddWithValue("@vrijeme_do", el.vrijeme_do)
                    mycmd.Parameters.AddWithValue("@evd_podnio", pEmpId)
                    mycmd.Parameters.AddWithValue("@evd_locked", _evdLock)
                    mycmd.Prepare()

                    mycmd.ExecuteNonQuery()
                Next

            Catch ex As Exception

                Console.WriteLine(ex.Message)
            End Try



        End Using

    End Sub

    Public Function isCreated() As Boolean

        Dim _retVal As Boolean = False

        Dim _countDays As Integer = 0
        For Each el In Me.EvidDnevnik
            If el.keyday Then _countDays += 1
        Next

        Dim _numDays = System.DateTime.DaysInMonth(Me.YYYY, Me.MM)
        If _countDays > 0 Then _retVal = True

        Return _retVal
    End Function

    Public Function isLocked() As Boolean

        Dim _retVal As Boolean = True

        For Each el In Me.EvidDnevnik
            _retVal = _retVal And el.locked
        Next

        Return True
    End Function

    Public Sub setLock(ByVal pLock As Boolean, Optional ByVal pSuperLock As Boolean = False)
        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString
        Dim strSQL As String
        Dim DtList As New DataTable

        strSQL = <![CDATA[ 
UPDATE evd_scheddnevnik ed
SET ed.evd_locked = @evd_locked
WHERE
ed.employeeID = @employeeID
AND MONTH(ed.datum) = @MM
AND YEAR(ed.datum) = @YYYY
AND ed.evd_locked < @evd_superlock;
    ]]>.Value

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySql.Data.MySqlClient.MySqlCommand

            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL


            Dim _evdLock As Integer = IIf(pLock, 1, 0)


            Dim _evdSuperLock As Integer = IIf(pSuperLock, 3, 2)


            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@employeeID", Me.EmployeeID)
            mycmd.Parameters.AddWithValue("@MM", Me.MM)
            mycmd.Parameters.AddWithValue("@YYYY", Me.YYYY)
            mycmd.Parameters.AddWithValue("@evd_locked", _evdLock)
            mycmd.Parameters.AddWithValue("@evd_superlock", _evdSuperLock)
            mycmd.Prepare()

            Try
                mycmd.ExecuteNonQuery()
            Catch ex As Exception

            End Try

        End Using

    End Sub

    Public Sub setLockExt()

        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString
        Dim strSQL As String
        Dim DtList As New DataTable

        strSQL = <![CDATA[ 
UPDATE
  evd_scheddnevnik_ext
SET
  evd_podnio = @evd_podnio,
  evd_odobrio = @evd_odobrio,
  evd_kontrolisao = @evd_kontrolisao,
  evd_stopped = @evd_stopped,
  evd_locked = @evd_locked
WHERE 
employeeID = @employeeID
AND mjesec = @mjesec
AND godina = @godina;
    ]]>.Value

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySql.Data.MySqlClient.MySqlCommand

            mycmd.Connection = myconnection

            Try

                mycmd.CommandText = strSQL
    

                mycmd.Parameters.Clear()

                Me.EvdlockedExt = IIf(Me.EvdPodnio > 0 AndAlso Me.EvdKontrolisao > 0, Me.EvdlockedExt, False)

                mycmd.Parameters.AddWithValue("@evd_podnio", Me.EvdPodnio)
                mycmd.Parameters.AddWithValue("@evd_odobrio", Me.EvdOdobrio)
                mycmd.Parameters.AddWithValue("@evd_kontrolisao", Me.EvdKontrolisao)
                mycmd.Parameters.AddWithValue("@evd_stopped", Me.EvdStopped)
                mycmd.Parameters.AddWithValue("@evd_locked", IIf(Me.EvdlockedExt, 1, 0))

                mycmd.Parameters.AddWithValue("@employeeID", Me.EmployeeID)
                mycmd.Parameters.AddWithValue("@mjesec", Me.MM)
                mycmd.Parameters.AddWithValue("@godina", Me.YYYY)

                mycmd.Prepare()

                mycmd.ExecuteNonQuery()

            Catch ex As Exception
                Console.WriteLine(ex.Message)
            End Try



        End Using

    End Sub

    Public Sub ins2Evidencije_OLD()
        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString
        Dim strSQL As String
        Dim DtList As New DataTable

        strSQL = <![CDATA[ 
INSERT INTO evd_scheduledata (
  ProgramName,
  ProgramStartTime,
  ProgramEndTime,
  OwnerId
)
SELECT 
IFNULL(ed.opis_rada,'RRD') AS ProgramName,
CONCAT(ed.datum,' ',ed.vrijeme_od) AS ProgramStartTime,
CONCAT(ed.datum,' ',ed.vrijeme_do) AS ProgramEndTime,
ed.employeeID AS OwnerId
FROM
evd_scheddnevnik ed
WHERE
ed.employeeID = @employeeID
AND MONTH(ed.datum) = @MM
AND YEAR(ed.datum) = @YYYY
AND (ed.sifra_placanja='RRD' OR ed.sifra_placanja='SP');



INSERT INTO evd_prisustva (
  employeeID,
  sifra_placanja,
  datum,
  vrijeme_od,
  vrijeme_do,
  vrijeme_ukupno
)
SELECT 
ed.employeeID,
ed.sifra_placanja,
ed.datum,
ed.vrijeme_od,
ed.vrijeme_do,
SEC_TO_TIME(TIME_TO_SEC(ed.vrijeme_do)-TIME_TO_SEC(ed.vrijeme_od)) AS vrijeme_ukupno
FROM
evd_scheddnevnik ed
WHERE
ed.employeeID = @employeeID
AND MONTH(ed.datum) = @MM
AND YEAR(ed.datum) = @YYYY
AND ed.sifra_placanja<>'RRD'
AND ed.sifra_placanja<>'SND'
AND ed.sifra_placanja<>'SP';

INSERT INTO evd_prisustva (
  employeeID,
  sifra_placanja,
  datum,
  vrijeme_od,
  vrijeme_do,
  vrijeme_ukupno
)
SELECT 
ed.employeeID,
ed.sifra_placanja,
ed.datum,
'08:00',
'16:00',
'08:00' AS vrijeme_ukupno
FROM
evd_scheddnevnik ed
WHERE
ed.employeeID = @employeeID
AND MONTH(ed.datum) = @MM
AND YEAR(ed.datum) = @YYYY
AND ed.sifra_placanja='SP';
    ]]>.Value

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySql.Data.MySqlClient.MySqlCommand
            'Dim myda As MySqlClient.MySqlDataAdapter

            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL


            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@employeeID", Me.EmployeeID)
            mycmd.Parameters.AddWithValue("@MM", Me.MM)
            mycmd.Parameters.AddWithValue("@YYYY", Me.YYYY)
            mycmd.Prepare()

            Try
                mycmd.ExecuteNonQuery()
            Catch ex As Exception

            End Try

        End Using

    End Sub

    Public Sub ins2Evidencije()
        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString
        Dim strSQL As String
        Dim DtList As New DataTable

        strSQL = <![CDATA[ 
CALL EVDwb_ins2evid(@employeeID, @MM, @YYYY);
    ]]>.Value

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySql.Data.MySqlClient.MySqlCommand
            'Dim myda As MySqlClient.MySqlDataAdapter

            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL


            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@employeeID", Me.EmployeeID)
            mycmd.Parameters.AddWithValue("@MM", Me.MM)
            mycmd.Parameters.AddWithValue("@YYYY", Me.YYYY)
            mycmd.Prepare()

            Try
                mycmd.ExecuteNonQuery()
            Catch ex As Exception

            End Try

        End Using

    End Sub

    Public Sub del2Evidencije()
        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString
        Dim strSQLes, strSQLep As String
        Dim DtList As New DataTable

        strSQLes = <![CDATA[ 
DELETE es.*
FROM evd_scheduledata es
WHERE
es.OwnerId = @employeeID
AND MONTH(es.ProgramStartTime) = @MM
AND YEAR(es.ProgramStartTime) = @YYYY;
    ]]>.Value

        strSQLep = <![CDATA[ 
DELETE ep.*
FROM evd_prisustva ep
WHERE 
ep.employeeID = @employeeID
AND MONTH(ep.datum) =  @MM
AND YEAR(ep.datum) = @YYYY;
    ]]>.Value

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySql.Data.MySqlClient.MySqlCommand
            'Dim myda As MySqlClient.MySqlDataAdapter

            mycmd.Connection = myconnection

            mycmd.CommandText = strSQLes


            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@employeeID", Me.EmployeeID)
            mycmd.Parameters.AddWithValue("@MM", Me.MM)
            mycmd.Parameters.AddWithValue("@YYYY", Me.YYYY)
            mycmd.Prepare()

            Try
                mycmd.ExecuteNonQuery()
            Catch ex As Exception
                Console.WriteLine(ex.Message)
            End Try

            mycmd.CommandText = strSQLep


            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@employeeID", Me.EmployeeID)
            mycmd.Parameters.AddWithValue("@MM", Me.MM)
            mycmd.Parameters.AddWithValue("@YYYY", Me.YYYY)
            mycmd.Prepare()

            Try
                mycmd.ExecuteNonQuery()
            Catch ex As Exception
                Console.WriteLine(ex.Message)
            End Try

        End Using

    End Sub



    ' API L2
    ' byorg + bypg
    Public Function getEvidDnStatusByOrgPG(ByVal pLstOrg As String, ByVal pLstPG As String) As List(Of NgEvidDnStatus)

        '
        ' Uzmi org.id iz sys_usr_role_json tabele za usera koji ima email: my_aspnet_membership.Email
        '
        'Dim _lstOrg As String = getOrgJed(pEmail)


        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString
        Dim strSQL As String
        Dim myda As MySqlClient.MySqlDataAdapter
        Dim DtList As New DataTable

        strSQL = <![CDATA[ 
SELECT
IFNULL(eds.`evd_sched_id`,-1) AS id,
e.`employeeID`,
e.`FirstName` AS  empFirstName,
e.`LastName` AS  empLastName,
IFNULL(eds.`godina`, @YYYY) AS YYYY,
IFNULL(eds.`mjesec`, @MM) AS MM,
IFNULL(eds.`evd_podnio`,0) AS evd_podnio,
IFNULL(eds.`evd_odobrio`,0) AS evd_odobrio,
IFNULL(eds.`evd_kontrolisao`,0) AS evd_kontrolisao,
IFNULL(eds.`evd_stopped`,0) AS evd_stopped,
IFNULL(eds.`evd_locked`,0) AS locked_ext 
FROM  `evd_employees` e
LEFT JOIN `evd_scheddnevnik_ext` eds ON eds.`employeeID`=e.`EmployeeID`
AND (eds.`godina`= @YYYY AND eds.`mjesec` = @MM)
WHERE 
(e.DepartmentUP IN 
(SELECT so.id 
FROM sfr_organizacija so
WHERE 
so.Sifra 
IN(%__ListOrgJed__%))
OR
e.`EmployeeID` IN 
(%__ListPGJed__%)
)
AND e.`Aktivan` <>0
ORDER BY e.`LastName`, e.`FirstName`, e.`employeeID`;

    ]]>.Value



        Using myconnection As New MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySqlCommand

            mycmd.Connection = myconnection
            pLstOrg = IIf(pLstOrg = "", "NULL", pLstOrg)
            strSQL = strSQL.Replace("%__ListOrgJed__%", pLstOrg)

            strSQL = strSQL.Replace("%__ListPGJed__%", pLstPG)


            mycmd.CommandText = strSQL


            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@MM", Me.MM)
            mycmd.Parameters.AddWithValue("@YYYY", Me.YYYY)
            mycmd.Prepare()

            DtList.Clear()
            myda = New MySqlDataAdapter(mycmd)
            myda.Fill(DtList)


            Return JsonConvert.DeserializeObject(Of List(Of NgEvidDnStatus))(ngApiGeneral.GetJson(DtList))

        End Using

    End Function

    ' API L3
    ' byorg + bypg
    Public Function getEvidDnStatusById(ByRef pEvidDnStatusLst As List(Of NgEvidDnStatus)) As List(Of NgEvidDnStatus)

        '
        ' Uzmi org.id iz sys_usr_role_json tabele za usera koji ima email: my_aspnet_membership.Email
        '
        'Dim _lstOrg As String = getOrgJed(pEmail)


        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString
        Dim strSQL As String
        Dim myda As MySqlClient.MySqlDataAdapter
        Dim DtList As New DataTable

        strSQL = <![CDATA[ 
SELECT
eds.evd_sched_id AS id,
eds.employeeID,
e.FirstName AS  empFirstName,
e.LastName AS  empLastName,
eds.godina AS YYYY,
eds.mjesec AS MM,
IFNULL(eds.evd_podnio,0) AS evd_podnio,
IFNULL(eds.evd_odobrio,0) AS evd_odobrio,
IFNULL(eds.evd_kontrolisao,0) AS evd_kontrolisao,
IFNULL(eds.`evd_stopped`,0) AS evd_stopped,
IFNULL(eds.evd_locked,0) AS locked_ext 
FROM evd_scheddnevnik_ext eds
INNER JOIN evd_employees e ON eds.employeeID=e.EmployeeID
WHERE 
eds.evd_sched_id IN (%__ListIds__%)
ORDER BY e.LastName;

    ]]>.Value

        Dim _ids As String = ""

        If pEvidDnStatusLst.Count > 0 Then

            For i As Integer = 0 To pEvidDnStatusLst.Count - 1
                If i <> pEvidDnStatusLst.Count - 1 Then
                    _ids += pEvidDnStatusLst.Item(i).id.ToString + ","
                Else
                    _ids += pEvidDnStatusLst.Item(i).id.ToString
                End If
            Next
        Else
            _ids = "-1"
        End If

        Using myconnection As New MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySqlCommand
            mycmd.Connection = myconnection

            strSQL = strSQL.Replace("%__ListIds__%", _ids)

            mycmd.CommandText = strSQL


            DtList.Clear()
            myda = New MySqlDataAdapter(mycmd)
            myda.Fill(DtList)

            Return JsonConvert.DeserializeObject(Of List(Of NgEvidDnStatus))(ngApiGeneral.GetJson(DtList))

            End Using

    End Function

    Public Sub updEvidDnevnikStatus(ByRef pEvidDnStatusLst As List(Of NgEvidDnStatus))

        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString
        Dim strSQL As String
        Dim DtList As New DataTable

        strSQL = <![CDATA[ 
UPDATE
  evd_scheddnevnik_ext
SET
  evd_odobrio = @evd_odobrio,
  evd_kontrolisao = @evd_kontrolisao,
  evd_stopped =  @evd_stopped,
  evd_locked = @evd_locked
WHERE 
evd_sched_id = @evd_sched_id
AND  employeeID = @employeeID;
    ]]>.Value

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySql.Data.MySqlClient.MySqlCommand

            mycmd.Connection = myconnection
            mycmd.CommandText = strSQL

            Try
                For Each el In pEvidDnStatusLst
        
                    el.locked_ext = IIf(el.evd_podnio > 0 AndAlso el.evd_kontrolisao > 0, el.locked_ext, False)

                    mycmd.Parameters.Clear()
                    mycmd.Parameters.AddWithValue("@id", el.id)
                    mycmd.Parameters.AddWithValue("@evd_odobrio", el.evd_odobrio)
                    mycmd.Parameters.AddWithValue("@evd_kontrolisao", el.evd_kontrolisao)
                    mycmd.Parameters.AddWithValue("@evd_stopped", el.evd_stopped)
                    mycmd.Parameters.AddWithValue("@evd_locked", el.locked_ext)
                    mycmd.Parameters.AddWithValue("@evd_sched_id", el.id)
                    mycmd.Parameters.AddWithValue("@employeeID", el.employeeID)
                    mycmd.Prepare()

                    mycmd.ExecuteNonQuery()
                Next

            Catch ex As Exception
                Console.WriteLine(ex.Message)
            End Try


        End Using

    End Sub
End Class
