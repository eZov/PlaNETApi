Imports Microsoft.VisualBasic
Imports System
Imports System.Collections.Generic
Imports System.ComponentModel
Imports System.Data
Imports System.Drawing
Imports System.Linq
Imports System.Text
Imports RouteGeoFence
Imports System.Globalization
Imports MySql.Data
Imports System.Web.Configuration
Imports MySql.Data.MySqlClient
Imports Newtonsoft.Json
Imports System.IO
'Imports MySql.Data.MySqlClient

Public Class ngApiEvidPrisDolaska


    Private points As List(Of RouteGeoFence.Point) = New List(Of RouteGeoFence.Point)()

    Public Sub New()

    End Sub

    Public Sub New(ByVal pFileName As String)
        loadData(pFileName)
    End Sub

    Public Sub New(ByRef pEvidPrisDolaska As NgEvidPrisDolaska)
        loadData(getGeoLoc(pEvidPrisDolaska))
    End Sub

    Public Sub loadData(ByVal pFileName As String)
        Dim ds As DataSet = New DataSet()
        Dim loc = HttpContext.Current.Server.MapPath("~/Data/" + pFileName)

        ds.ReadXml(loc)

        For Each dr As DataRow In ds.Tables(0).Rows
            Dim p As RouteGeoFence.Point = New RouteGeoFence.Point()

            Dim Lat As String = dr(0).ToString()
            p.X = Double.Parse(Lat, CultureInfo.InvariantCulture)

            Dim _Long As String = dr(1).ToString()
            p.Y = Double.Parse(_Long, CultureInfo.InvariantCulture)
            points.Add(p)
        Next
    End Sub

    Public Sub loadData(ByVal pXmlReader As TextReader)

        Dim ds As DataSet = New DataSet()
        ds.ReadXml(pXmlReader)

        For Each dr As DataRow In ds.Tables(0).Rows
            Dim p As RouteGeoFence.Point = New RouteGeoFence.Point()

            Dim Lat As String = dr(0).ToString()
            p.X = Double.Parse(Lat, CultureInfo.InvariantCulture)

            Dim _Long As String = dr(1).ToString()
            p.Y = Double.Parse(_Long, CultureInfo.InvariantCulture)
            points.Add(p)
        Next
    End Sub

    Public Function getGeoLoc(ByRef pEvidPrisDolaska As NgEvidPrisDolaska) As TextReader


        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString
        Dim strSQL As String
        Dim DtList As New DataTable

        Dim xe As TextReader

        Dim pEmpId As Integer = -1


        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySqlCommand
            Dim dbRead As MySqlDataReader
            'Dim myda As MySqlDataAdapter

            mycmd.Connection = myconnection

            strSQL = <![CDATA[ 
SELECT gl.`Points`
FROM `sfr_geolocation` gl
LEFT JOIN `evd_employees` e ON e.`OfficeLocation`=gl.`Naziv`
WHERE e.`EmployeeID`=@EmployeeId;
    ]]>.Value

            mycmd.CommandText = strSQL


            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@EmployeeId", pEvidPrisDolaska.EmployeeId)
            mycmd.Prepare()

            Try
                dbRead = mycmd.ExecuteReader
                If dbRead.HasRows Then
                    While dbRead.Read
                        xe = dbRead.GetTextReader(0)
                    End While
                End If
                dbRead.Close()

            Catch ex As Exception
                Console.WriteLine(ex.Message)
            End Try

        End Using


        Return xe

    End Function

    Public Function checkGeoLoc(ByVal txtLat As String, ByVal txtLong As String) As Boolean

        Dim myRoute As ClsPolyGon = New ClsPolyGon(points)
        Dim stat As Boolean = myRoute.FindPoint(Double.Parse(txtLat, CultureInfo.InvariantCulture), Double.Parse(txtLong, CultureInfo.InvariantCulture))

        Return stat

    End Function

    Public Function selEvidPrisIN(ByRef pEvidPrisDolaska As NgEvidPrisDolaska) As NgEvidPrisDolaska


        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString
        Dim strSQL As String
        Dim DtList As New DataTable


        Dim pEmpId As Integer = -1


        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySqlCommand
            Dim dbRead As MySqlDataReader
            Dim myda As MySqlDataAdapter

            mycmd.Connection = myconnection

            strSQL = <![CDATA[ 
SELECT ed.`Id`, ed.`EmployeeId`, DATE_FORMAT(ed.`InTime`,'%Y-%m-%d %H:%i:%s') AS InTime, ed.`InGeoloc`, 
ed.`OutTime`, ed.`OutGeoloc`, ed.`OnLocation`, ed.`Duration`, ed.`Opis`
FROM `evd_pris_dolaska` ed
WHERE ed.`OutTime` IS NULL
AND ed.`OnLocation` <> 0
AND DATEDIFF( ed.`InTime`, NOW()) >= -1
AND ed.`EmployeeId` = @EmployeeId;
    ]]>.Value

            mycmd.CommandText = strSQL


            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@EmployeeId", pEvidPrisDolaska.EmployeeId)
            mycmd.Prepare()

            Try

                DtList.Clear()
                myda = New MySqlDataAdapter(mycmd)
                myda.Fill(DtList)

            Catch ex As Exception
                Console.WriteLine(ex.Message)
            End Try

        End Using

        Try
            If DtList.Rows.Count > 0 Then
                Dim _listEvid As List(Of NgEvidPrisDolaska) = JsonConvert.DeserializeObject(Of List(Of NgEvidPrisDolaska))(ngApiGeneral.GetJson(DtList))
                Return _listEvid.Item(0)
            Else
                pEvidPrisDolaska.OnLocation = False
                Return pEvidPrisDolaska
            End If
        Catch ex As Exception
            Console.WriteLine(ex.Message)
        End Try


        Return Nothing

    End Function

    Public Function selEvidPrisRec(ByRef pEvidPrisDolaska As NgEvidPrisDolaska, Optional ByVal pNumRecs As Integer = 1) As List(Of NgEvidPrisDolaska)


        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString
        Dim strSQL As String
        Dim DtList As New DataTable


        Dim pEmpId As Integer = -1


        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySqlCommand
            'Dim dbRead As MySqlDataReader
            Dim myda As MySqlDataAdapter

            mycmd.Connection = myconnection

            strSQL = <![CDATA[ 
SELECT   
  ed.`Id`,
  ed.`EmployeeId`,
  DATE_FORMAT(ed.`InTime`,'%Y-%m-%d %H:%i:%s') AS InTime,
  ed.`InGeoloc`,
  DATE_FORMAT(ed.`OutTime`,'%Y-%m-%d %H:%i:%s') AS OutTime,
  ed.`OutGeoloc`,
  ed.`OnLocation`,
  IFNULL(ed.`Duration`, '00:00:00') AS Duration,
  ed.`Opis`
FROM `evd_pris_dolaska` ed
WHERE ed.`EmployeeId` = @EmployeeId
ORDER BY ed.`DummyTime` DESC
LIMIT 0, @NumRecs;
    ]]>.Value

            mycmd.CommandText = strSQL


            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@EmployeeId", pEvidPrisDolaska.EmployeeId)
            mycmd.Parameters.AddWithValue("@NumRecs", pNumRecs)
            mycmd.Prepare()

            Try

                DtList.Clear()
                myda = New MySqlDataAdapter(mycmd)
                myda.Fill(DtList)

            Catch ex As Exception
                Console.WriteLine(ex.Message)
            End Try

        End Using

        If DtList.Rows.Count = 0 Then
            Dim _lstEvidPrisDolaska As New List(Of NgEvidPrisDolaska)
            pEvidPrisDolaska.Opis = "nema prijava"
            _lstEvidPrisDolaska.Add(pEvidPrisDolaska)
            Return _lstEvidPrisDolaska
        End If

        Return JsonConvert.DeserializeObject(Of List(Of NgEvidPrisDolaska))(ngApiGeneral.GetJson(DtList))

    End Function

    Public Sub insEvidPrisIn(ByRef pEvidPrisDolaska As NgEvidPrisDolaska)


        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString
        Dim strSQL As String
        Dim DtList As New DataTable


        Dim pEmpId As Integer = -1


        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySql.Data.MySqlClient.MySqlCommand
            mycmd.Connection = myconnection

            strSQL = <![CDATA[ 
INSERT INTO `evd_pris_dolaska` ( `EmployeeId`,  `InTime`,  `InGeoloc`, `OnLocation`, `Opis`)
VALUES  (   @EmployeeId, @InTime, @InGeoloc, @OnLocation, @Opis);
    ]]>.Value

            pEvidPrisDolaska.InTime = Format(Now, "yyyy-MM-dd HH:mm:ss")

            mycmd.CommandText = strSQL


            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@EmployeeId", pEvidPrisDolaska.EmployeeId)
            mycmd.Parameters.AddWithValue("@InTime", pEvidPrisDolaska.InTime)
            mycmd.Parameters.AddWithValue("@InGeoloc", pEvidPrisDolaska.InGeoloc)
            mycmd.Parameters.AddWithValue("@OnLocation", pEvidPrisDolaska.OnLocation)
            mycmd.Parameters.AddWithValue("@Opis", "prijava")
            mycmd.Prepare()

            Try

                mycmd.ExecuteNonQuery()

            Catch ex As Exception
                Console.WriteLine(ex.Message)
            End Try

        End Using



    End Sub

    Public Sub insEvidPrisOut(ByRef pEvidPrisDolaska As NgEvidPrisDolaska)


        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString
        Dim strSQL As String
        Dim DtList As New DataTable


        Dim pEmpId As Integer = -1


        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySql.Data.MySqlClient.MySqlCommand
            mycmd.Connection = myconnection

            strSQL = <![CDATA[ 
UPDATE
  `evd_pris_dolaska`
SET
  `OutTime` = @OutTime,
  `OutGeoloc` = @OutGeoloc,
  `OnLocation` = @OnLocation,
  `Opis` = @Opis
WHERE `Id` = @Id
AND `EmployeeId` = @EmployeeId;

UPDATE `evd_pris_dolaska` ed
SET ed.`Duration`=TIMEDIFF(ed.`OutTime`,ed.`InTime`)
WHERE ed.`Id`=@Id AND ed.`EmployeeId`=@EmployeeId;
    ]]>.Value

            pEvidPrisDolaska.OutTime = Format(Now, "yyyy-MM-dd HH:mm:ss")

            mycmd.CommandText = strSQL

            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@Id", pEvidPrisDolaska.Id)
            mycmd.Parameters.AddWithValue("@EmployeeId", pEvidPrisDolaska.EmployeeId)
            mycmd.Parameters.AddWithValue("@OutTime", pEvidPrisDolaska.OutTime)
            mycmd.Parameters.AddWithValue("@OutGeoloc", pEvidPrisDolaska.OutGeoloc)
            mycmd.Parameters.AddWithValue("@OnLocation", pEvidPrisDolaska.OnLocation)
            mycmd.Parameters.AddWithValue("@Opis", pEvidPrisDolaska.Opis)
            mycmd.Prepare()

            Try

                mycmd.ExecuteNonQuery()

            Catch ex As Exception
                Console.WriteLine(ex.Message)
            End Try

        End Using



    End Sub

End Class
