Imports System.Data
Imports Microsoft.VisualBasic
Imports MySql.Data
Imports MySql.Data.MySqlClient

Public Class ClsObracuni

    Public Sub New()

        DtList = New DataTable
        DtList.TableName = "dtObracuni"

        DtVrstaList = New DataTable
        DtVrstaList.TableName = "dtVrstaObracuni"

        DtObrDetails = New DataTable

    End Sub

    Public Property DtList As DataTable

    Public Property DtVrstaList As DataTable

    Public Property DtObrDetails As DataTable
    Public Property DrObrDetails As DataRow

    Public Function getList(Optional ByRef pVrsta As String = "plc_net") As DataTable

        Dim myda As MySqlClient.MySqlDataAdapter

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim strSQL As String = <![CDATA[ 
SELECT
  id AS Id, 0 AS ParentId, CONCAT(Godina,'-',Mjesec,'  (',id,')',IF(Started<>0,' tekući','')) AS Naziv, 
  Vrsta, Mjesec, Godina, SatiUMjesecu, SatiOsnovica, OrgJed, VrijednostBoda_1, VrijednostBoda_2, 
  VrijednostTopli_1, DatumIsplate, DatumUplateDoprinosa, SumaPlacanja, SumaDoprinosi, SumaObustave, 
  SumaNet, Started, Completed, UserName, UserDate, DummyTimeStamp, DateFrom, DateTo, SatiDrPraznik, 
  ProsjPlaca, DatumObracuna, LastDone, PorOlaksice
FROM obr_obracuniheader 
WHERE Completed = 0
ORDER BY id ASC;
]]>.Value

            DtList.Clear()


            Select Case pVrsta
                Case "plc_net"
                    strSQL = <![CDATA[ 
Select 
  id AS Id, 0 As ParentId, CONCAT(Godina,'-',Mjesec,'  (',id,')',IF(Started<>0,' tekući','')) AS Naziv, 
  Vrsta, Mjesec, Godina, SatiUMjesecu, SatiOsnovica, OrgJed, VrijednostBoda_1, VrijednostBoda_2,
  VrijednostTopli_1, DatumIsplate, DatumUplateDoprinosa, SumaPlacanja, SumaDoprinosi, SumaObustave,
  SumaNet, Started, Completed, UserName, UserDate, DummyTimeStamp, DateFrom, DateTo, SatiDrPraznik,
  ProsjPlaca, DatumObracuna, LastDone, PorOlaksice
From obr_obracuniheader
Where Completed = 0
Order By id ASC;
]]>.Value


                Case "plc_net_arh"
                    strSQL = <![CDATA[ 
SELECT
  id AS Id, 0 AS ParentId, CONCAT(Godina,'-',LPAD(Mjesec,2,'0'),'  (',id,')',IF(Started<>0,' ','')) AS Naziv, 
  Vrsta, Mjesec, Godina, SatiUMjesecu, SatiOsnovica, OrgJed, VrijednostBoda_1, VrijednostBoda_2, 
  VrijednostTopli_1, DatumIsplate, DatumUplateDoprinosa, SumaPlacanja, SumaDoprinosi, SumaObustave, 
  SumaNet, Started, Completed, UserName, UserDate, DummyTimeStamp, DateFrom, DateTo, 0 AS SatiDrPraznik, 
  0 AS ProsjPlaca, DatumObracuna, 0 AS LastDone, PorOlaksice
FROM a_obracuniheader 
ORDER BY id ASC;
]]>.Value

                Case Else

            End Select


            myda = New MySqlClient.MySqlDataAdapter(strSQL, myconnection)
            myda.Fill(DtList)

        End Using

        Return DtList

    End Function

    Public Function getIdx(Optional ByVal pSfrReport As String = "000000") As Integer

        If pSfrReport.Length = 0 Then Return -1
        Dim idx As Integer = -1

        For Each er In DtList.Rows

            idx += 1
            If er("Id") = pSfrReport Then
                Exit For
            End If
        Next

        Return idx
    End Function

    Public Function getIdx(ByVal pIdReport As Integer) As Integer

        If pIdReport <= 0 Then Return -1
        Dim idx As Integer = -1

        For Each er In DtList.Rows

            idx += 1
            If er("id") = pIdReport Then
                Exit For
            End If
        Next

        Return idx
    End Function

    Public Function getSifra(ByVal pIdx As Integer) As String

        Dim idx As Integer = -1
        Dim retVal As String = ""

        For Each er In DtList.Rows
            idx += 1
            If idx = pIdx Then
                retVal = er("Id")
                Exit For
            End If
        Next

        Return retVal
    End Function

    Public Function getId(ByVal pIdx As Integer) As Integer

        Dim idx As Integer = -1
        Dim retVal As String = ""

        For Each er In DtList.Rows
            idx += 1
            If idx = pIdx Then
                retVal = CInt(er("hidId"))
                Exit For
            End If
        Next

        Return retVal
    End Function

    Public Function getVrstaList(Optional ByRef pVrsta As String = "plc_net") As DataTable

        Dim myda As MySqlClient.MySqlDataAdapter

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim strSQL As String = <![CDATA[ 
SELECT id, VrstaObracunaID, Sifra, Naziv, Atribut_1, Atribut_2, Atribut_3, Atribut_4, Atribut_5
FROM   sfr_obracunvrsta;
]]>.Value



            myda = New MySqlClient.MySqlDataAdapter(strSQL, myconnection)
            myda.Fill(DtVrstaList)

        End Using

        Return DtVrstaList

    End Function

    Public Function getVrstaIdx(ByVal pId As Integer) As Integer

        If pId <= 0 Then Return -1
        Dim idx As Integer = -1

        For Each er In DtVrstaList.Rows

            idx += 1
            If er("id") = pId Then
                Exit For
            End If
        Next

        Return idx
    End Function

    Public Function getVrstaId(ByVal pIdx As Integer) As Integer

        Dim idx As Integer = -1
        Dim retVal As String = ""

        For Each er In DtVrstaList.Rows
            idx += 1
            If idx = pIdx Then
                retVal = CInt(er("id"))
                Exit For
            End If
        Next

        Return retVal
    End Function

    Public Function getObrDetails(ByVal pObrId As String) As DataTable

        Dim myda As MySqlClient.MySqlDataAdapter
        Dim mycmd As MySqlCommand

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            mycmd = New MySqlCommand
            mycmd.Connection = myconnection

            Dim strSQL As String = <![CDATA[ 
SELECT
  id AS Id, 0 AS ParentId, CONCAT(Godina,'-',Mjesec,'  (',id,')') AS Naziv, 
  Vrsta, Mjesec, Godina, SatiUMjesecu, SatiOsnovica, OrgJed, VrijednostBoda_1, VrijednostBoda_2, 
  VrijednostTopli_1, DatumIsplate, DatumUplateDoprinosa, 
  IFNULL(SumaPlacanja,0) AS SumaPlacanja, 
  IFNULL(SumaDoprinosi,0) AS SumaDoprinosi, 
  IFNULL(SumaObustave,0) AS SumaObustave, 
  IFNULL(SumaNet,0) AS SumaNet, 
  Started, Completed, UserName, UserDate, DummyTimeStamp, DateFrom, DateTo, SatiDrPraznik, 
  ProsjPlaca, DatumObracuna, LastDone, PorOlaksice
FROM obr_obracuniheader 
WHERE id = @ObrId
ORDER BY Godina ASC, Mjesec ASC, id ASC;
]]>.Value
            mycmd.CommandText = strSQL

            mycmd.Parameters.AddWithValue("@ObrId", pObrId)
            mycmd.Prepare()

            DtObrDetails.Clear()
            myda = New MySqlClient.MySqlDataAdapter(mycmd)
            myda.Fill(DtObrDetails)



        End Using
        If DtObrDetails.Rows.Count > 0 Then
            DrObrDetails = DtObrDetails.Rows(0)
        End If



        Return DtObrDetails

    End Function

    Public Function ObrDetails() As ClsObracun

        Dim cObracun As New ClsObracun

        If DrObrDetails IsNot Nothing Then

            cObracun.Id = CInt(DrObrDetails.Item("id"))
            cObracun.IdVrsta = CInt(DrObrDetails.Item("Vrsta"))

            cObracun.Mjesec = CInt(DrObrDetails.Item("Mjesec"))
            cObracun.Godina = CInt(DrObrDetails.Item("Godina"))


            cObracun.SatiUMjesecu = CInt(DrObrDetails.Item("SatiUMjesecu"))
            cObracun.SatiOsnovica = CInt(DrObrDetails.Item("SatiOsnovica"))

            cObracun.OrgJed = CInt(DrObrDetails.Item("OrgJed"))

            cObracun.VriBoda1 = CDbl(DrObrDetails.Item("VrijednostBoda_1"))
            cObracun.VriBoda2 = CDbl(DrObrDetails.Item("VrijednostBoda_2"))
            cObracun.VriTopli = CDbl(DrObrDetails.Item("VrijednostTopli_1"))

            cObracun.DatIspl = DateTime.Parse(DrObrDetails("DatumIsplate").ToString())
            cObracun.DatDopr = DateTime.Parse(DrObrDetails("DatumUplateDoprinosa").ToString())

            cObracun.SumaPlac = CDbl(DrObrDetails.Item("SumaPlacanja"))
            cObracun.SumaDopr = CDbl(DrObrDetails.Item("SumaDoprinosi"))
            cObracun.SumaObust = CDbl(DrObrDetails.Item("SumaObustave"))
            cObracun.SumaNet = CDbl(DrObrDetails.Item("SumaNet"))

            cObracun.Started = CBool(DrObrDetails.Item("Started"))
            cObracun.Completed = CBool(DrObrDetails.Item("Completed"))

            cObracun.DateFrom = DateTime.Parse(DrObrDetails("DateFrom").ToString())
            cObracun.DateTo = DateTime.Parse(DrObrDetails("DateTo").ToString())

            cObracun.SatiDrzPraznik = CInt(DrObrDetails.Item("SatiDrPraznik"))
            cObracun.ProsjPlaca = CDbl(DrObrDetails.Item("ProsjPlaca"))

            cObracun.DatObr = DateTime.Parse(DrObrDetails("DatumObracuna").ToString())

            cObracun.LastDone = CBool(DrObrDetails.Item("LastDone"))
            cObracun.PorOlaksice = CBool(DrObrDetails.Item("PorOlaksice"))

        End If



        Return cObracun

    End Function

    Public Sub updateObr(ByRef pRows As Hashtable)

        Dim mycmd As MySqlCommand
        Dim rowStarted As Integer

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            mycmd = New MySqlCommand
            mycmd.Connection = myconnection

            Dim strSQL As String = <![CDATA[ 
UPDATE
 obr_obracuniheader
SET
  OrgJed = @OrgJed,
  Vrsta = @Vrsta, 
  Mjesec = @Mjesec, 
  Godina = @Godina, 
  DateFrom = @DateFrom, 
  DateTo = @DateTo,   
  DatumIsplate = @DatumIsplate,  
  DatumUplateDoprinosa = @DatumUplateDoprinosa,  
  DatumObracuna = @DatumObracuna,
  Started = @Started, 
  PorOlaksice = @PorOlaksice,  
  SatiUMjesecu = @SatiUMjesecu, 
  SatiOsnovica = @SatiOsnovica,  
  ProsjPlaca = @ProsjPlaca,   
  VrijednostBoda_1 = @VrijednostBoda_1, 
  VrijednostTopli_1 = @VrijednostTopli_1,   
  SatiDrPraznik = @SatiDrPraznik
WHERE id = @id;
]]>.Value
            mycmd.CommandText = strSQL

            'For Each pRows As DictionaryEntry In pRows
            Dim rowKey As Integer = CInt(pRows("id"))
            Dim rowOrgJed As String = CStr(pRows("OrgJed"))
            Dim rowVrsta As Double = CDbl(pRows("Vrsta"))
            Dim rowMjesec As Double = CDbl(pRows("Mjesec"))
            Dim rowGodina As Double = CDbl(pRows("Godina"))
            Dim rowDateFrom As String = CDate(pRows("DateFrom")).ToString("yyyy-MM-dd")
            Dim rowDateTo As String = CDate(pRows("DateTo")).ToString("yyyy-MM-dd")
            Dim rowDatumIsplate As String = CDate(pRows("DatumIsplate")).ToString("yyyy-MM-dd")
            Dim rowDatumUplateDoprinosa As String = CDate(pRows("DatumUplateDoprinosa")).ToString("yyyy-MM-dd")
            Dim rowDatumObracuna As String = CDate(pRows("DatumObracuna")).ToString("yyyy-MM-dd")
            rowStarted = CInt(pRows("Started"))
            Dim rowPorOlaksice As Integer = CInt(pRows("PorOlaksice"))
            Dim rowSatiUMjesecu As Double = CDbl(pRows("SatiUMjesecu"))
            Dim rowSatiOsnovica As Integer = CInt(pRows("SatiOsnovica"))
            Dim rowProsjPlaca As Double = CDbl(pRows("ProsjPlaca"))
            Dim rowVrijednostBoda_1 As Double = CDbl(pRows("VrijednostBoda_1"))
            Dim rowVrijednostTopli_1 As Double = CDbl(pRows("VrijednostTopli_1"))
            Dim rowSatiDrPraznik As Double = CDbl(pRows("SatiDrPraznik"))

            Try
                mycmd.Parameters.Clear()
                mycmd.Parameters.AddWithValue("@id", rowKey)
                mycmd.Parameters.AddWithValue("@OrgJed", rowOrgJed)
                mycmd.Parameters.AddWithValue("@Vrsta", rowVrsta)
                mycmd.Parameters.AddWithValue("@Mjesec", rowMjesec)
                mycmd.Parameters.AddWithValue("@Godina", rowGodina)
                mycmd.Parameters.AddWithValue("@DateFrom", rowDateFrom)
                mycmd.Parameters.AddWithValue("@DateTo", rowDateTo)
                mycmd.Parameters.AddWithValue("@DatumIsplate", rowDatumIsplate)
                mycmd.Parameters.AddWithValue("@DatumUplateDoprinosa", rowDatumIsplate)
                mycmd.Parameters.AddWithValue("@DatumObracuna", rowDatumObracuna)
                mycmd.Parameters.AddWithValue("@Started", rowStarted)
                mycmd.Parameters.AddWithValue("@PorOlaksice", rowPorOlaksice)
                mycmd.Parameters.AddWithValue("@SatiUMjesecu", rowSatiUMjesecu)
                mycmd.Parameters.AddWithValue("@SatiOsnovica", rowSatiOsnovica)
                mycmd.Parameters.AddWithValue("@ProsjPlaca", rowProsjPlaca)
                mycmd.Parameters.AddWithValue("@VrijednostBoda_1", rowVrijednostBoda_1)
                mycmd.Parameters.AddWithValue("@VrijednostTopli_1", rowVrijednostTopli_1)
                mycmd.Parameters.AddWithValue("@SatiDrPraznik", rowSatiDrPraznik)
                mycmd.Prepare()

                mycmd.ExecuteNonQuery()
            Catch ex As Exception
                ex.ToString()
            End Try


            'Next

        End Using

        rowStarted = CInt(pRows("Started"))
        If rowStarted <> 0 Then
            updateTekObr(pRows)
        End If

    End Sub

    Public Sub updateTekObr(ByRef pRows As Hashtable)

        Dim mycmd As MySqlCommand

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            mycmd = New MySqlCommand
            mycmd.Connection = myconnection

            Dim strSQL As String = <![CDATA[ 
UPDATE obr_obracuniheader
SET Started = 0;
UPDATE
 obr_obracuniheader
SET
  Started = @Started
WHERE id = @id;
]]>.Value
            mycmd.CommandText = strSQL

            'For Each pRows As DictionaryEntry In pRows
            Dim rowKey As Integer = CInt(pRows("id"))
            Dim rowStarted As Integer = CInt(pRows("Started"))

            Try
                mycmd.Parameters.Clear()
                mycmd.Parameters.AddWithValue("@id", rowKey)
                mycmd.Parameters.AddWithValue("@Started", rowStarted)
                mycmd.Prepare()

                mycmd.ExecuteNonQuery()
            Catch ex As Exception
                ex.ToString()
            End Try


            'Next

        End Using


    End Sub

    Public Sub insertObr(ByRef pRows As Hashtable)

        Dim mycmd As MySqlCommand
        Dim dbRead As MySqlClient.MySqlDataReader

        Dim _lastInsId As Integer = -1
        Dim _satiUMjes As Integer = -1

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            mycmd = New MySqlCommand
            mycmd.Connection = myconnection

            Dim strSQL As String = <![CDATA[ 
SELECT COUNT(c.`dt`)*8 AS SatiUMjes FROM `sfr_calendar` c
WHERE c.`isWeekday`=1 AND c.`m`=MONTH(NOW()) AND c.y=YEAR(NOW());
]]>.Value

            mycmd.CommandText = strSQL

            dbRead = mycmd.ExecuteReader
            Try
                Do While dbRead.Read
                    _satiUMjes = dbRead.GetInt32("SatiUMjes")
                Loop
            Catch ex As Exception
                ex.ToString()
            End Try
            dbRead.Close()

            strSQL = <![CDATA[ 
INSERT INTO obr_obracuniheader (
  Vrsta, Mjesec, Godina, SatiUMjesecu,
  OrgJed, 
  VrijednostBoda_1, VrijednostTopli_1, 
  DatumIsplate, DatumUplateDoprinosa, 
  DateFrom, DateTo, 
  DatumObracuna
)
VALUES
  (
    1, 
    MONTH(NOW()), YEAR(NOW()), @SatiUMjesecu,
    @OrgJed, 
    1, 1, 
    CURDATE(), CURDATE(),  
    ADDDATE(MAKEDATE(YEAR(CURDATE()),1), INTERVAL MONTH(NOW())-1 MONTH), 
    LAST_DAY(MAKEDATE(YEAR(CURDATE()),DAYOFYEAR(CURDATE()))),
    CURDATE()
  );
]]>.Value
            mycmd.CommandText = strSQL

            Dim rowOrgJed As String = CStr(pRows("OrgJed"))

            Try
                mycmd.Parameters.Clear()
                mycmd.Parameters.AddWithValue("@OrgJed", rowOrgJed)
                mycmd.Parameters.AddWithValue("@SatiUMjesecu", _satiUMjes.ToString)
                mycmd.Prepare()

                mycmd.ExecuteNonQuery()

                _lastInsId = mycmd.LastInsertedId
                'pRows.Add("id", _lastInsId.ToString)

            Catch ex As Exception
                ex.ToString()
            End Try


            'Next

        End Using


    End Sub

    Public Function tekuciObr() As Integer

        Dim mycmd As MySqlCommand
        Dim dbRead As MySqlClient.MySqlDataReader

        Dim _tekuciObr As Integer = -1

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            mycmd = New MySqlCommand
            mycmd.Connection = myconnection

            Dim strSQL As String = <![CDATA[ 
SELECT oh.`id` FROM `obr_obracuniheader` oh
WHERE oh.`Started` <> 0;
]]>.Value

            mycmd.CommandText = strSQL

            dbRead = mycmd.ExecuteReader
            Try
                Do While dbRead.Read
                    _tekuciObr = dbRead.GetInt32("id")
                Loop
            Catch ex As Exception
                ex.ToString()
            End Try
            dbRead.Close()

        End Using

        Return _tekuciObr

    End Function

    Public Function execObr(Optional ByVal pFinObr As Boolean = False) As Integer

        Dim mycmd As MySqlCommand
        Dim strSQL As String = ""
        Dim retValue As Integer = -1

        Dim pIdObr As Integer = Me.tekuciObr

        Select Case pFinObr
            Case True
                '
                ' FINALNI obračun
                '
                strSQL = <![CDATA[ 
CALL OBR_Procedure_Test( @IdObr, 1);
]]>.Value
            Case False
                '
                ' KONTROLNI obračun
                '
                strSQL = <![CDATA[ 
CALL OBR_Procedure_Test( @IdObr, 0);
]]>.Value

            Case Else

        End Select

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            mycmd = New MySqlCommand
            mycmd.Connection = myconnection


            mycmd.CommandText = strSQL


            Try
                mycmd.Parameters.Clear()
                mycmd.Parameters.AddWithValue("@IdObr", pIdObr)
                mycmd.Prepare()

                retValue = mycmd.ExecuteNonQuery()
            Catch ex As Exception
                ex.ToString()
            End Try


            'Next

        End Using

        Return retValue

    End Function

    Public Function isWorking() As Boolean

        Dim dbRead As MySqlClient.MySqlDataReader
        Dim mycmd As MySqlCommand

        Dim _retValue As Boolean = False

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            mycmd = New MySqlCommand
            mycmd.Connection = myconnection

            Dim strSQL As String = <![CDATA[ 
  SELECT COUNT(log_log) AS Working FROM sys_log WHERE log_log='WORKING';  
]]>.Value
            mycmd.CommandText = strSQL

            Dim _Working As Integer = 0

            dbRead = mycmd.ExecuteReader
            Try
                Do While dbRead.Read
                    _Working = dbRead.GetInt32("Working")
                Loop
            Catch ex As Exception
                ex.ToString()
            End Try
            dbRead.Close()

            If _Working > 0 Then _retValue = True

        End Using


        Return _retValue

    End Function
End Class
