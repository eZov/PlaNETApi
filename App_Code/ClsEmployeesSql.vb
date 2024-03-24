Imports System.Data
Imports Microsoft.VisualBasic
Imports MySql.Data
Imports MySql.Data.MySqlClient

Public Class ClsEmployeesSql

    Public Shared Sub StazGO(ByVal pEmployeeId As Integer, ByVal pDate As String,
                             ByRef pGod As String, ByRef pMje As String, ByRef pDan As String)

        If pEmployeeId <= 0 Then Exit Sub

        Dim sql As String = ""

        Dim mycmd As MySqlCommand

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            mycmd = New MySqlCommand
            mycmd.Connection = myconnection

            Try

                mycmd.CommandText = "GO_StazGOEmp"
                mycmd.CommandType = CommandType.StoredProcedure

                mycmd.Parameters.AddWithValue("@pEmployee", pEmployeeId)
                mycmd.Parameters("@pEmployee").Direction = ParameterDirection.Input
                mycmd.Parameters.AddWithValue("@pDATE", pDate)
                mycmd.Parameters("@pDATE").Direction = ParameterDirection.Input

                mycmd.Parameters.AddWithValue("@pSUkG", MySqlDbType.String)
                mycmd.Parameters("@pSUkG").Direction = ParameterDirection.Output

                mycmd.Parameters.AddWithValue("@pSUkM", MySqlDbType.String)
                mycmd.Parameters("@pSUkM").Direction = ParameterDirection.Output

                mycmd.Parameters.AddWithValue("@pSUkD", MySqlDbType.String)
                mycmd.Parameters("@pSUkD").Direction = ParameterDirection.Output

                mycmd.ExecuteNonQuery()

                pGod = mycmd.Parameters("@pSUkG").Value
                pMje = mycmd.Parameters("@pSUkM").Value
                pDan = mycmd.Parameters("@pSUkD").Value

            Catch ex As Exception
                Dim el As New ErrorsAndEvents.ErrorLogger
                el.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
            End Try

        End Using




    End Sub

    Public Shared Sub StazGOKont(ByVal pEmployeeId As Integer, ByVal pDate As String)

        If pEmployeeId <= 0 Then Exit Sub

        Dim sql As String = ""

        Dim mycmd As MySqlCommand

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            mycmd = New MySqlCommand
            mycmd.Connection = myconnection

            Try

                mycmd.CommandText = "GO_StazGOKontEmp"
                mycmd.CommandType = CommandType.StoredProcedure

                mycmd.Parameters.AddWithValue("@pEmployee", pEmployeeId)
                mycmd.Parameters("@pEmployee").Direction = ParameterDirection.Input
                mycmd.Parameters.AddWithValue("@pDATE", pDate)
                mycmd.Parameters("@pDATE").Direction = ParameterDirection.Input

                mycmd.ExecuteNonQuery()

            Catch ex As Exception
                Dim el As New ErrorsAndEvents.ErrorLogger
                el.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
            End Try

        End Using

    End Sub

    Public Shared Sub IzracunajDaneGO(ByVal pEmployeeId As Integer)

        If pEmployeeId <= 0 Then Exit Sub

        Dim sql As String = ""

        Dim mycmd As MySqlCommand

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            mycmd = New MySqlCommand
            mycmd.Connection = myconnection

            Try

                mycmd.CommandText = "GO_IzracunajDaneEmp"
                mycmd.CommandType = CommandType.StoredProcedure

                mycmd.Parameters.AddWithValue("@pEmployeeID", pEmployeeId)
                mycmd.Parameters("@pEmployeeID").Direction = ParameterDirection.Input

                mycmd.ExecuteNonQuery()

            Catch ex As Exception
                Dim el As New ErrorsAndEvents.ErrorLogger
                el.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
            End Try

        End Using

    End Sub

    Public Shared Sub PripremaRjesenjaGO(ByVal pEmployeeId As Integer, ByVal pGodina As Integer)

        If pEmployeeId <= 0 Then Exit Sub

        Dim sql As String = ""

        Dim mycmd As MySqlCommand

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            mycmd = New MySqlCommand
            mycmd.Connection = myconnection

            Try

                mycmd.CommandText = "GO_PripremaRjesenjaEmp"
                mycmd.CommandType = CommandType.StoredProcedure

                mycmd.Parameters.AddWithValue("@pEmployee", pEmployeeId)
                mycmd.Parameters("@pEmployee").Direction = ParameterDirection.Input

                mycmd.Parameters.AddWithValue("@pGodina", pGodina)
                mycmd.Parameters("@pGodina").Direction = ParameterDirection.Input

                mycmd.ExecuteNonQuery()

            Catch ex As Exception
                Dim el As New ErrorsAndEvents.ErrorLogger
                el.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
            End Try

        End Using

    End Sub

    'Public Shared Function CountEndDate(ByRef dtpStartDate As Date, ByVal numDays As Integer) As Date
    '    Dim count As Integer = 0
    '    Dim countWork As Integer = 0
    '    Dim startDate As Date = dtpStartDate
    '    Dim endDate As Date = Nothing
    '    Dim endDateFin As Date = Nothing
    '    'Dim totalDays = (dtpEndDate - dtpStartDate).Days
    '    Try
    '        For i As Integer = 0 To 100
    '            Dim weekday As DayOfWeek = startDate.AddDays(i).DayOfWeek
    '            ' Kontrola praznika - varijabla DayIsHoliday

    '            endDate = startDate.AddDays(i)
    '            If weekday = DayOfWeek.Saturday Or weekday = DayOfWeek.Sunday Or gCalendar.GetDay(endDate).IsHoliday Then
    '                count += 1
    '            ElseIf (countWork >= numDays) Then
    '                Exit For
    '            Else
    '                countWork += 1
    '            End If
    '            'If i > numDays Then Exit For
    '        Next

    '        endDate = startDate.AddDays(count + numDays - 1)

    '    Catch ex As Exception

    '    End Try



    '    For i As Integer = 0 To 10 Step 1
    '        endDateFin = endDate.AddDays(-i)
    '        Dim weekday As DayOfWeek = endDateFin.DayOfWeek
    '        ' Kontrola praznika - varijabla DayIsHoliday


    '        If weekday = DayOfWeek.Saturday Or weekday = DayOfWeek.Sunday Or gCalendar.GetDay(endDateFin).IsHoliday Then
    '            count += 1
    '        Else
    '            Exit For
    '        End If
    '        'If i > numDays Then Exit For
    '    Next



    '    Return endDateFin

    'End Function

    'Public Shared Function CountNextWorkDay(ByRef dtpStartDate As Date) As Integer
    '    Dim count = 0
    '    Dim startDate As Date = dtpStartDate
    '    Dim endDate As Date

    '    'Dim totalDays = (dtpEndDate - dtpStartDate).Days
    '    Try

    '        For i As Integer = 1 To 100
    '            Dim weekday As DayOfWeek = startDate.AddDays(i).DayOfWeek
    '            ' Kontrola praznika - varijabla DayIsHoliday

    '            endDate = startDate.AddDays(i)
    '            If weekday = DayOfWeek.Saturday Or weekday = DayOfWeek.Sunday Or gCalendar.GetDay(endDate).IsHoliday Then
    '                count += 1
    '            Else
    '                Exit For
    '            End If
    '            'If i > numDays Then Exit For
    '        Next

    '        endDate = startDate.AddDays(count)
    '    Catch ex As Exception

    '    End Try

    '    Return count + 1
    'End Function

    Public Shared Function PreklapanjeGO(ByVal pEmployeeId As Integer,
                                        ByVal pSDate As Date, ByVal pEDate As Date) As Boolean

        Dim _Result As Boolean = False

        Dim sql As String = ""

        Dim mycmd As MySqlCommand

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            mycmd = New MySqlCommand
            mycmd.Connection = myconnection


            Try
                mycmd.CommandText = "GO_PreklapanjeGO"
                mycmd.CommandType = CommandType.StoredProcedure
                mycmd.Parameters.AddWithValue("@pEmployeeId", pEmployeeId)
                mycmd.Parameters("@pEmployeeId").Direction = ParameterDirection.Input

                mycmd.Parameters.AddWithValue("@pSDate", pSDate)
                mycmd.Parameters("@pSDate").Direction = ParameterDirection.Input
                mycmd.Parameters.AddWithValue("@pEDate", pEDate)
                mycmd.Parameters("@pEDate").Direction = ParameterDirection.Input

                mycmd.Parameters.AddWithValue("@pResult", MySqlDbType.String)
                mycmd.Parameters("@pResult").Direction = ParameterDirection.Output
                mycmd.ExecuteNonQuery()

                Dim iResult As Integer = mycmd.Parameters("@pResult").Value
                If iResult > 0 Then _Result = True
            Catch ex As Exception
                Dim el As New ErrorsAndEvents.ErrorLogger
                el.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
            End Try


        End Using



        Return _Result

    End Function

End Class
