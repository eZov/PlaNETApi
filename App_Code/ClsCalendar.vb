Imports System.Data
Imports Microsoft.VisualBasic
Imports MySql.Data
Imports MySql.Data.MySqlClient

Public Class ClsCalendar

    Private dsCalendar As New Data.DataSet
    Private dtCalendar As Data.DataTable

    Private dsPraznici As New Data.DataSet
    Private dtPraznici As Data.DataTable

    Public Sub New()

    End Sub

    Public ReadOnly Property selMonth() As Data.DataTable
        Get
            Return dtCalendar
        End Get
    End Property

    Public ReadOnly Property Praznici() As Data.DataTable
        Get
            Return dtPraznici
        End Get
    End Property

    Public Sub getMonth(ByVal pEmployeeID As Integer, ByVal datum As System.DateTime)

        Dim myda As MySqlClient.MySqlDataAdapter

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)

            myconnection.Open()

            Dim sYear As String = datum.Year.ToString
            Dim sMonth As String = datum.Month.ToString
            Dim sDays As String = Date.DaysInMonth(datum.Year, datum.Month)

            Dim sDate As String = sYear & "/" & sMonth & "/01"
            Dim eDate As String = sYear & "/" & sMonth & "/" & sDays

            Dim strSQL As String = " SELECT *  FROM evd_prisustva  WHERE employeeID=" &
                                    pEmployeeID & " AND datum >= '" & sDate & "' AND datum <= '" & eDate & "';"

            dsCalendar.Clear()

            myda = New MySqlClient.MySqlDataAdapter(strSQL, myconnection)
            dsCalendar.Clear()
            myda.Fill(dsCalendar, "dtMonth")
            dtCalendar = dsCalendar.Tables("dtMonth")

        End Using
    End Sub

    Public Sub getPraznici(ByVal pYear As Integer, ByVal pMonth As Integer)

        Dim mycmd As MySql.Data.MySqlClient.MySqlCommand
        Dim myda As MySqlClient.MySqlDataAdapter

        Dim strSQL As String

        ' Tekuća godina
        '

        strSQL = <![CDATA[ 
SELECT sc.dt AS datum FROM sfr_calendar sc WHERE sc.y=@pYear AND sc.m=@pMonth AND sc.`isHoliday`=1;
]]>.Value

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            mycmd = New MySqlCommand
            mycmd.Connection = myconnection

            strSQL = strSQL.Replace("@pYear", pYear)
            strSQL = strSQL.Replace("@pMonth", pMonth)
            'mycmd.Parameters.AddWithValue("@pYear", pYear)

            myda = New MySqlClient.MySqlDataAdapter(strSQL, myconnection)
            dsPraznici.Clear()
            myda.Fill(dsPraznici, "dtPraznici")
            dtPraznici = dsPraznici.Tables("dtPraznici")

        End Using
    End Sub

    Public Function getGodineGO() As DataTable

        Dim myda As MySqlClient.MySqlDataAdapter
        Dim DtGodineGO As New DataTable
        DtGodineGO.TableName = "DtGodineGO"

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim strSQL As String = <![CDATA[ 
SELECT DISTINCT c.`y` AS Id,  c.`y` AS Naziv FROM `sfr_calendar` c
WHERE c.y<=YEAR(NOW()) AND c.y>YEAR(NOW())-2 ORDER BY c.y DESC;
]]>.Value


            DtGodineGO.Clear()
            myda = New MySqlClient.MySqlDataAdapter(strSQL, myconnection)
            myda.Fill(DtGodineGO)

        End Using

        Return DtGodineGO

    End Function

    Public Function CountGOEndDate(ByRef dtpStartDate As Date, ByVal numDays As Integer) As List(Of Date)

        Dim mycmd As MySqlCommand
        Dim myrdr As MySqlDataReader

        Dim myLstDate As New List(Of Date)

        If numDays <= 0 Then Return myLstDate

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim strSQL As String = <![CDATA[ 
SELECT sc.dt AS datum FROM sfr_calendar sc WHERE sc.dt >=DATE('@stDate') AND (sc.`isHoliday`=0 AND sc.`isWeekday`=1)
LIMIT @numDays,2;
]]>.Value

            Try
                '
                '   Konverzija iz DateTime u MySqlDateTime !!
                '
                strSQL = strSQL.Replace("@stDate", dtpStartDate.ToString("yyyy-MM-dd"))
                strSQL = strSQL.Replace("@numDays", (numDays - 1).ToString)

                mycmd = New MySqlCommand(strSQL, myconnection)
                myrdr = mycmd.ExecuteReader

                While myrdr.Read
                    '
                    '
                    '   Konverzija iz MySqlDateTime u DateTime
                    myLstDate.Add(DateTime.Parse(myrdr.GetMySqlDateTime("datum").ToString))
                End While
            Catch ex As Exception

            End Try

        End Using

        Return myLstDate

    End Function
End Class


'Public Class ClsCalendarDay

'    Public Property CalDate As Date
'    Public Property CalYear As Integer
'    Public Property CalMonth As Integer
'    Public Property CalDay As Integer
'    Public Property CalQuarter As Integer
'    Public Property CalDayOfWeek As Integer
'    Public Property IsWeekDay As Boolean
'    Public Property IsHoliday As Boolean

'End Class
