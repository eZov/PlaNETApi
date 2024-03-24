Imports Microsoft.VisualBasic
Imports MySql.Data
Imports MySql.Data.MySqlClient

Public Class ClsYearMonth

    Private cMonth As Integer
    Private cYear As Integer

    Public Property listMonth As New List(Of ClsYearMonth)
    Public Property listYear As New List(Of ClsYearMonth)

    Public Sub New()

    End Sub

    Public Sub New(ByVal pId As Integer, ByVal pText As String)
        ID = pId
        Text = pText
    End Sub

    Public Property ID As Integer
    Public Property Text As String

    Public Property selMonthIdx As Integer
    Public Property selYearIdx As Integer

    Private Sub getMonthYearObr()

        Dim mycmd As MySqlCommand
        Dim myrdr As MySqlDataReader

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            mycmd = New MySqlCommand
            mycmd.Connection = myconnection

            Dim strSQL As String

            strSQL = <![CDATA[ 
SELECT oh.`Mjesec`, oh.`Godina` FROM `obr_obracuniheader` oh WHERE oh.`Started` <>0;;
]]>.Value
            mycmd.CommandText = strSQL


            myrdr = mycmd.ExecuteReader
            If myrdr.HasRows Then
                While myrdr.Read
                    cMonth = myrdr("Mjesec")
                    cYear = myrdr("Godina")
                End While
            Else
                cMonth = Date.Now.Month
                cYear = Date.Now.Year
            End If
        End Using

    End Sub

    Public Sub getMonthsYears(Optional pSelValueNow As Boolean = False)

        If pSelValueNow Then
            cMonth = Now().Month
            cYear = Now().Year
        Else
            getMonthYearObr()
        End If


        'Dim _dataMonth As New List(Of ClsYearMonth)

        listMonth.Add(New ClsYearMonth(1, "01"))
        listMonth.Add(New ClsYearMonth(2, "02"))
        listMonth.Add(New ClsYearMonth(3, "03"))
        listMonth.Add(New ClsYearMonth(4, "04"))
        listMonth.Add(New ClsYearMonth(5, "05"))
        listMonth.Add(New ClsYearMonth(6, "06"))
        listMonth.Add(New ClsYearMonth(7, "07"))
        listMonth.Add(New ClsYearMonth(8, "08"))
        listMonth.Add(New ClsYearMonth(9, "09"))
        listMonth.Add(New ClsYearMonth(10, "10"))
        listMonth.Add(New ClsYearMonth(11, "11"))
        listMonth.Add(New ClsYearMonth(12, "12"))

        Dim _idx As Integer = 0
        For Each el As ClsYearMonth In listMonth
            If el.ID = cMonth Then
                selMonthIdx = _idx
                Exit For
            End If
            _idx += 1
        Next

        Dim _startYear = 2018
        Dim _numOfYear As Integer = 10

        For _iRow As Integer = 0 To _numOfYear

            listYear.Add(New ClsYearMonth(_startYear + _iRow, (_startYear + _iRow).ToString))

        Next

        'Dim _dataYear As New List(Of ClsYearMonth)
        'listYear.Add(New ClsYearMonth(2018, "2018"))
        'listYear.Add(New ClsYearMonth(2019, "2019"))
        'listYear.Add(New ClsYearMonth(2020, "2020"))
        'listYear.Add(New ClsYearMonth(2021, "2021"))

        _idx = 0
        For Each el As ClsYearMonth In listYear
            If el.ID = cYear Then
                selYearIdx = _idx
                Exit For
            End If
            _idx += 1
        Next


    End Sub

    Public Function getMonthByIdx(ByVal pIdx As Integer) As Integer

        Dim _idx As Integer = 0
        For Each el As ClsYearMonth In listMonth
            If pIdx = _idx Then
                Return el.ID
                Exit For
            End If
            _idx += 1
        Next

        Return -1

    End Function

    Public Function getYearByIdx(ByVal pIdx As Integer) As Integer

        Dim _idx As Integer = 0
        For Each el As ClsYearMonth In listYear
            If pIdx = _idx Then
                Return el.ID
                Exit For
            End If
            _idx += 1
        Next

        Return -1

    End Function
End Class
