Imports Microsoft.VisualBasic
Imports MySql.Data
Imports System.Xml
Imports System.Xml.Linq
Imports MySql.Data.MySqlClient
Imports System.Collections.Generic


Public Class ClsSchedulerDnRd
    Private dsCalendar As New Data.DataSet
    Private dtCalendar As Data.DataTable

    Private insDtMonth As New List(Of ClsSchedulerOne)
    Private savDtMonth As New List(Of ClsSchedulerOne)

    Public Sub New()

    End Sub

    Public ReadOnly Property selMonth() As Data.DataTable
        Get
            Return dtCalendar
        End Get
    End Property

    Public ReadOnly Property prepInsMonth As List(Of ClsSchedulerOne)
        Get
            Return insDtMonth
        End Get
    End Property

    Public ReadOnly Property prepSavMonth As List(Of ClsSchedulerOne)
        Get
            Return savDtMonth
        End Get
    End Property


    Public Sub getMonth(ByVal pEmployeeID As Integer, ByVal datum As System.DateTime)

        Dim myda As MySqlClient.MySqlDataAdapter

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()


            Dim sYear As String = datum.Year.ToString
            Dim sMonth As String = datum.Month.ToString
            Dim sDays As String = Date.DaysInMonth(datum.Year, datum.Month)

            Dim startDate As String = sYear & "/" & sMonth & "/01"
            Dim endDate As String = sYear & "/" & sMonth & "/" & sDays

            Dim strSQL As String = <![CDATA[ 
 SELECT DISTINCT -1 AS ProgramId, '' AS ProgramName, '' AS Comments,
 DAY(sc.dt) AS schedDay,
 '' AS ProgramStartTime,
 '' AS ProgramEndTime, 
 0 AS IsAllDay,
 1 AS CategorizeId,
 ?OwnerId AS OwnerId
  FROM `sfr_calendar` sc WHERE sc.y=?year AND sc.m=?month
 AND sc.dt NOT IN(SELECT DATE(es.ProgramStartTime) FROM evd_scheduledata_dnrd es 
 WHERE es.OwnerId = ?OwnerId AND es.ProgramStartTime >= '?ProgramStartTime' AND es.ProgramEndTime <= '?ProgramEndTime')
 UNION
SELECT `ProgramId`, `ProgramName`, `Comments`, DAY(ProgramStartTime) AS schedDay, 
        IF(TIME(ProgramStartTime)='23:59:00','', DATE_FORMAT(ProgramStartTime,'%H:%i')) AS ProgramStartTime,
        IF(TIME(ProgramStartTime)='23:59:00','', DATE_FORMAT(ProgramEndTime,'%H:%i')) AS ProgramEndTime, 
        IsAllDay, `CategorizeId`, `OwnerId` FROM `evd_scheduledata_dnrd` 
        WHERE OwnerId = ?OwnerId 
        AND ProgramStartTime >= '?ProgramStartTime' AND ProgramEndTime <= '?ProgramEndTime'  ORDER BY schedDay, ProgramStartTime;]]>.Value


            strSQL = strSQL.Replace("?OwnerId", pEmployeeID)
            strSQL = strSQL.Replace("?ProgramStartTime", startDate)
            strSQL = strSQL.Replace("?ProgramEndTime", endDate)
            strSQL = strSQL.Replace("?year", sYear)
            strSQL = strSQL.Replace("?month", sMonth)


            dsCalendar.Clear()

            myda = New MySqlClient.MySqlDataAdapter(strSQL, myconnection)
            myda.Fill(dsCalendar, "dtMonth")
            dtCalendar = dsCalendar.Tables(0)

        End Using

    End Sub

    Public Sub delDayOfMonth(ByVal pEmployeeID As Integer, ByVal pST As String, ByVal pID As Integer)

        Dim mycmd As MySqlCommand

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()


            Try
                mycmd = New MySqlCommand
                mycmd.Connection = myconnection

                Dim strSQL As String = <![CDATA[ 
DELETE FROM evd_scheduledata_dnrd
WHERE DATE(ProgramStartTime) = DATE(@ProgramStartTime) AND
  OwnerId = @OwnerId AND ProgramId = @ProgramId;
    ]]>.Value


                mycmd.CommandText = strSQL
    

                mycmd.Parameters.AddWithValue("@ProgramId", pID)
                mycmd.Parameters.AddWithValue("@ProgramStartTime", pST)
                mycmd.Parameters.AddWithValue("@OwnerId", pEmployeeID)
                mycmd.Prepare()

                mycmd.ExecuteNonQuery()
            Catch ex As Exception

            End Try

        End Using

    End Sub


    Public Sub insDayOfMonth(ByVal pEmployeeID As Integer, ByVal pST As String, ByVal pET As String, ByVal pOpis As String, _
                              Optional ByVal pComments As String = "")

        Dim mycmd As MySqlCommand

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Try
                mycmd = New MySqlCommand
                mycmd.Connection = myconnection

                Dim strSQL As String = <![CDATA[ 
    INSERT INTO evd_scheduledata_dnrd (ProgramName, Comments, ProgramStartTime, ProgramEndTime, IsAllDay, CategorizeId, OwnerId)
    VALUES ( @ProgramName, @Comments, @ProgramStartTime, @ProgramEndTime,  @IsAllDay,  @CategorizeId,  @OwnerId); 
    ]]>.Value


                mycmd.CommandText = strSQL
    


                mycmd.Parameters.AddWithValue("@ProgramName", pOpis)
                mycmd.Parameters.AddWithValue("@Comments", pComments)
                mycmd.Parameters.AddWithValue("@ProgramStartTime", pST)
                mycmd.Parameters.AddWithValue("@ProgramEndTime", pET)
                mycmd.Parameters.AddWithValue("@IsAllDay", 0)
                mycmd.Parameters.AddWithValue("@CategorizeId", 1)
                mycmd.Parameters.AddWithValue("@OwnerId", pEmployeeID)
                mycmd.Prepare()

                mycmd.ExecuteNonQuery()
            Catch ex As Exception

            End Try


        End Using

    End Sub

    Public Sub saveDayOfMonth(ByVal pEmployeeID As Integer, ByVal pST As String, ByVal pET As String, ByVal pOpis As String, ByVal pID As Integer, _
                              Optional ByVal pComments As String = "")
        Dim mycmd As MySqlCommand

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()


            Try
                mycmd = New MySqlCommand
                mycmd.Connection = myconnection

                Dim strSQL As String = <![CDATA[ 
UPDATE evd_scheduledata_dnrd
SET
  ProgramName = @ProgramName,
  Comments = @Comments,
  ProgramStartTime = @ProgramStartTime,
  ProgramEndTime = @ProgramEndTime,
  IsAllDay = @IsAllDay,
  CategorizeId = @CategorizeId,
  OwnerId = @OwnerId
WHERE ProgramId = @ProgramId;
    ]]>.Value


                mycmd.CommandText = strSQL
    

                mycmd.Parameters.AddWithValue("@ProgramId", pID)
                mycmd.Parameters.AddWithValue("@ProgramName", pOpis)
                mycmd.Parameters.AddWithValue("@Comments", pComments)
                mycmd.Parameters.AddWithValue("@ProgramStartTime", pST)
                mycmd.Parameters.AddWithValue("@ProgramEndTime", pET)
                mycmd.Parameters.AddWithValue("@IsAllDay", 0)
                mycmd.Parameters.AddWithValue("@CategorizeId", 1)
                mycmd.Parameters.AddWithValue("@OwnerId", pEmployeeID)
                mycmd.Prepare()

                mycmd.ExecuteNonQuery()
            Catch ex As Exception

            End Try
        End Using


    End Sub

    Public Sub delRRN(ByVal pEmployeeID As Integer, ByVal pYear As Integer, ByVal pMonth As Integer)

        Dim mycmd As MySqlCommand

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Try
                mycmd = New MySqlCommand
                mycmd.Connection = myconnection

                Dim strSQL As String = <![CDATA[ 
DELETE ep.* FROM evd_prisustva ep 
WHERE ep.employeeID=@pEmployeeID AND MONTH(ep.datum)=@pMonth AND YEAR(ep.datum)=@pYear
AND DAYOFWEEK(ep.`datum`)=1 AND  sifra_placanja='RRN';
    ]]>.Value


                mycmd.CommandText = strSQL
    

                mycmd.Parameters.AddWithValue("@pEmployeeID", pEmployeeID)
                mycmd.Parameters.AddWithValue("@pYear", pYear)
                mycmd.Parameters.AddWithValue("@pMonth", pMonth)
                mycmd.Prepare()

                mycmd.ExecuteNonQuery()
            Catch ex As Exception

            End Try

        End Using



    End Sub

    Public Sub insRRN(ByVal pEmployeeID As Integer, ByVal pST As String)

        Dim mycmd As MySqlCommand

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()



            Try
                mycmd = New MySqlCommand
                mycmd.Connection = myconnection

                Dim strSQL As String = <![CDATA[ 
INSERT INTO `evd_prisustva` (`employeeID`, `sifra_placanja`, `datum`, `vrijeme_od`, `vrijeme_do`, `vrijeme_ukupno`)
SELECT @pEmployeeID, 'RRN', DATE(@pST),'08:00','16:00','08:00';
    ]]>.Value


                mycmd.CommandText = strSQL
    

                mycmd.Parameters.AddWithValue("@pEmployeeID", pEmployeeID)
                mycmd.Parameters.AddWithValue("@pST", pST)
                mycmd.Prepare()

                mycmd.ExecuteNonQuery()
            Catch ex As Exception

            End Try


        End Using


    End Sub


    Public Sub insMonth()

        Dim mycmd As MySqlCommand


        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Try
                mycmd = New MySqlCommand
                mycmd.Connection = myconnection

                Dim strSQL As String = <![CDATA[ 
    INSERT INTO evd_scheduledata_dnrd (ProgramName, Comments, ProgramStartTime, ProgramEndTime, IsAllDay, CategorizeId, OwnerId)
    VALUES ( @ProgramName, @Comments, @ProgramStartTime, @ProgramEndTime,  @IsAllDay,  @CategorizeId,  @OwnerId); 
    ]]>.Value


                mycmd.CommandText = strSQL
    

                For Each el As ClsSchedulerOne In prepInsMonth

                    mycmd.Parameters.Clear()
                    mycmd.Parameters.AddWithValue("@ProgramName", el.pOpis)
                    mycmd.Parameters.AddWithValue("@Comments", el.pComments)
                    mycmd.Parameters.AddWithValue("@ProgramStartTime", el.pST)
                    mycmd.Parameters.AddWithValue("@ProgramEndTime", el.pET)
                    mycmd.Parameters.AddWithValue("@IsAllDay", 0)
                    mycmd.Parameters.AddWithValue("@CategorizeId", 1)
                    mycmd.Parameters.AddWithValue("@OwnerId", el.pOwnerId)
                    mycmd.Prepare()

                    mycmd.ExecuteNonQuery()
                Next


            Catch ex As Exception

            End Try
        End Using





        'myconnection.Close()
    End Sub

    Public Sub savMonth()

        Dim mycmd As MySqlCommand


        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()



            Try
                mycmd = New MySqlCommand
                mycmd.Connection = myconnection

                Dim strSQL As String = <![CDATA[ 
UPDATE evd_scheduledata_dnrd
SET
  ProgramName = @ProgramName,
  Comments = @Comments,
  ProgramStartTime = @ProgramStartTime,
  ProgramEndTime = @ProgramEndTime,
  IsAllDay = @IsAllDay,
  CategorizeId = @CategorizeId,
  OwnerId = @OwnerId
WHERE ProgramId = @ProgramId;
    ]]>.Value


                mycmd.CommandText = strSQL
    

                For Each el As ClsSchedulerOne In prepSavMonth

                    mycmd.Parameters.Clear()
                    mycmd.Parameters.AddWithValue("@ProgramId", el.pID)
                    mycmd.Parameters.AddWithValue("@ProgramName", el.pOpis)
                    mycmd.Parameters.AddWithValue("@Comments", el.pComments)
                    mycmd.Parameters.AddWithValue("@ProgramStartTime", el.pST)
                    mycmd.Parameters.AddWithValue("@ProgramEndTime", el.pET)
                    mycmd.Parameters.AddWithValue("@IsAllDay", 0)
                    mycmd.Parameters.AddWithValue("@CategorizeId", 1)
                    mycmd.Parameters.AddWithValue("@OwnerId", el.pOwnerId)
                    mycmd.Prepare()

                    mycmd.ExecuteNonQuery()
                Next

            Catch ex As Exception

            End Try

        End Using


    End Sub

End Class
