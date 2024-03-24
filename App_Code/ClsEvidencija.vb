Imports Microsoft.VisualBasic
Imports System.Data.OleDb
Imports System.Data.SqlClient
Imports MySql.Data
Imports System.Data
Imports System.Collections
Imports System
Imports System.Web.UI.WebControls

Public Class Evidencija

#Region "**** Insert ****"


    Public Shared Sub Insert_Sati(ByVal datum As String, ByVal employeeID As Integer, ByVal selVrstaPlacanja As String)
        Dim cnData As New MySql.Data.MySqlClient.MySqlConnection
        Dim lcmd As MySql.Data.MySqlClient.MySqlCommand

        If IsInsertDozvoljen_Sati(datum, employeeID, selVrstaPlacanja) = False Then
            Exit Sub
        End If

        If selVrstaPlacanja = "P20" Then
            If IsGODaniRaspolozivo_Sati(employeeID) <= 0 Then
                Exit Sub
            End If
        End If
        ' Dodaj u bazu evidenciju za ovaj dan
        cnData.ConnectionString = ConnectionString
        cnData.Open()
        Dim insSQL As String = "INSERT INTO evd_prisustva (employeeID, sifra_placanja, datum, vrijeme_od, vrijeme_do, vrijeme_ukupno)	VALUES(" & employeeID.ToString & ", '" & selVrstaPlacanja & "','" & datum & "','08:00:00', '16:00:00', '08:00:00');"
        Dim delSQL As String = "DELETE FROM evd_prisustva WHERE datum = '" & datum & "' AND employeeID=" & employeeID.ToString & ";"
        lcmd = New MySql.Data.MySqlClient.MySqlCommand(delSQL, cnData)
        ' lcmd.CommandText = insSQL
        Try
            If selVrstaPlacanja = "DELETE" Then
                lcmd.ExecuteNonQuery()
            Else
                lcmd.ExecuteNonQuery()
                lcmd.CommandText = insSQL
                lcmd.ExecuteNonQuery()
            End If
        Catch ex As Exception

        End Try

        cnData.Close()
    End Sub


    Public Shared Sub InsertByRukovodilac_Sati(ByVal datum As String, ByVal employeeID As Integer, ByVal selVrstaPlacanja As String, ByVal employeeRukovodilac As Integer)
        Dim cnData As New MySql.Data.MySqlClient.MySqlConnection
        Dim lcmd As MySql.Data.MySqlClient.MySqlCommand

        If IsOdobrenoBezUposlenika_Sati(datum, employeeID) Then
            Exit Sub
        End If

        If selVrstaPlacanja.Length < 3 Then
            Exit Sub
        End If

        If selVrstaPlacanja = "P20" Then
            If IsGODaniRaspolozivo_Sati(employeeID) <= 0 Then
                Exit Sub
            End If
        End If


        'Dim cDatum As String() = datum.Split(New Char() {"/"c})

        ' Odobri evidencije prethodno unijete - a koje nisu odobrene
        Odobrenje_EvidencijaByRole(Now.Year, Now.Month, employeeID, employeeRukovodilac, eRoles.uposlenik.ToString)

        ' Dodaj u bazu evidenciju za ovaj dan
        cnData.ConnectionString = ConnectionString
        cnData.Open()
        Dim insSQL As String = "INSERT INTO evd_prisustva (employeeID, sifra_placanja, datum, vrijeme_ukupno, evd_podnio)	VALUES(" & employeeID.ToString & ", '" & selVrstaPlacanja & "','" & datum & "','08:00:00'," & employeeRukovodilac.ToString & ");"
        Dim delSQL As String = "DELETE FROM evd_prisustva WHERE datum = '" & datum & "' AND employeeID=" & employeeID.ToString & ";"
        lcmd = New MySql.Data.MySqlClient.MySqlCommand(delSQL, cnData)
        ' lcmd.CommandText = insSQL
        Try
            If selVrstaPlacanja = "DELETE" Then
                lcmd.ExecuteNonQuery()
            Else
                lcmd.ExecuteNonQuery()
                lcmd.CommandText = insSQL
                lcmd.ExecuteNonQuery()
            End If
        Catch ex As Exception

        End Try

        cnData.Close()
    End Sub

    Public Shared Sub InsertByUprava_Sati(ByVal datum As Date, ByVal employeeID As Integer, ByVal selVrstaPlacanja As String, ByVal employeeRukovodilac As Integer)
        Dim cnData As New MySql.Data.MySqlClient.MySqlConnection
        Dim lcmd As MySql.Data.MySqlClient.MySqlCommand

        If IsOdobrenoBezUposlenika_Sati(datum, employeeID) Then
            'Exit Sub
        End If

        If selVrstaPlacanja.Length < 3 Then
            Exit Sub
        End If

        If selVrstaPlacanja = "P20" Then
            If IsGODaniRaspolozivo_Sati(employeeID) <= 0 Then
                'Exit Sub
            End If
        End If

        'Dim cDatum As String() = datum.Split(New Char() {"/"c})

        ' Odobri evidencije prethodno unijete - a koje nisu odobrene
        Odobrenje_EvidencijaByRole(datum.Year, datum.Month, employeeID, employeeRukovodilac, eRoles.uposlenik.ToString)

        ' Dodaj u bazu evidenciju za ovaj dan
        cnData.ConnectionString = ConnectionString
        cnData.Open()
        Dim insSQL As String = "INSERT INTO evd_prisustva (employeeID, sifra_placanja, datum, vrijeme_ukupno, evd_podnio)	VALUES(" & employeeID.ToString & ", '" & selVrstaPlacanja & "','" & Evidencija.Date_MySQLFormat(datum) & "','08:00:00'," & employeeRukovodilac.ToString & ");"
        Dim delSQL As String = "DELETE FROM evd_prisustva WHERE datum = '" & Evidencija.Date_MySQLFormat(datum) & "' AND employeeID=" & employeeID.ToString & ";"
        lcmd = New MySql.Data.MySqlClient.MySqlCommand(delSQL, cnData)
        ' lcmd.CommandText = insSQL
        Try
            If selVrstaPlacanja = "DELETE" Then
                lcmd.ExecuteNonQuery()
            Else
                lcmd.ExecuteNonQuery()
                lcmd.CommandText = insSQL
                lcmd.ExecuteNonQuery()
            End If
        Catch ex As Exception

        End Try

        cnData.Close()
    End Sub

    Public Shared Sub InsertByEvidencija_Sati(ByVal datum As Date, ByVal employeeID As Integer, ByVal selVrstaPlacanja As String, ByVal employeeEvidencija As Integer)

        Dim cDatabase As ClsDatabase = New ClsDatabase

        If IsOdobrenoBezUposlenikaIRukovodioca_Sati(datum, employeeID) Then
            Exit Sub
        End If

        If selVrstaPlacanja.Length < 3 Then
            Exit Sub
        End If

        If selVrstaPlacanja = "P20" Then
            If IsGODaniRaspolozivo_Sati(employeeID) <= 0 Then
                Exit Sub
            End If
        End If

        'Dim cDatum As String() = datum.Split(New Char() {"/"c})
        '' Odobri evidencije prethodno unijete - a koje nisu odobrene
        'Odobrenje_EvidencijaByRole(CInt(cDatum(0)), CInt(cDatum(1)), employeeID, employeeEvidencija, eRoles.rukovodilac.ToString)
        Odobrenje_EvidencijaByRole(datum.Year, datum.Month, employeeID, employeeEvidencija, eRoles.rukovodilac.ToString)

        ' Dodaj u bazu evidenciju za ovaj dan

        Dim _datumStr As String = ClsDatabase.DBDate(datum)

        'Dim insSQL As String = "INSERT INTO evd_prisustva (employeeID, sifra_placanja, datum, vrijeme_ukupno, evd_podnio, evd_odobrio)	VALUES(" & employeeID.ToString & ", '" & selVrstaPlacanja & "','" & _datumStr & "','08:00:00', " & employeeEvidencija.ToString & "," & employeeEvidencija.ToString & ");"
        'Dim delSQL As String = "DELETE FROM evd_prisustva WHERE datum = '" & _datumStr & "' AND employeeID=" & employeeID.ToString & ";"

        Dim insSQL As String = <![CDATA[ 
INSERT INTO evd_prisustva (employeeID, sifra_placanja, datum, vrijeme_ukupno, evd_podnio, evd_odobrio)	VALUES(?employeeID, '?selVrstaPlacanja',?_datumStr,'08:00:00', ?employeeEvidencija,?employeeEvidencija);
]]>.Value
        insSQL = insSQL.Replace("?employeeID", employeeID.ToString)
        insSQL = insSQL.Replace("?selVrstaPlacanja", selVrstaPlacanja)
        insSQL = insSQL.Replace("?_datumStr", _datumStr)
        insSQL = insSQL.Replace("?employeeEvidencija", employeeEvidencija.ToString)

        Dim delSQL As String = <![CDATA[ 
DELETE FROM evd_prisustva WHERE datum = ?_datumStr AND employeeID=?employeeID;
]]>.Value
        insSQL = insSQL.Replace("?employeeID", employeeID.ToString)
        insSQL = insSQL.Replace("?_datumStr", _datumStr)

        Try
            If selVrstaPlacanja = "DELETE" Then
                cDatabase.sqlExecute(delSQL)
            Else
                cDatabase.sqlExecute(delSQL)
                cDatabase.sqlExecute(insSQL)
            End If
        Catch ex As Exception

        End Try

    End Sub

    Public Shared Sub InsertBezBrisanja_Sati(ByVal datum As String, ByVal employeeID As Integer, ByVal selVrstaPlacanja As String)
        Dim cnData As New MySql.Data.MySqlClient.MySqlConnection
        Dim lcmd As MySql.Data.MySqlClient.MySqlCommand

        If IsInsertDozvoljen_Sati(datum, employeeID, selVrstaPlacanja) = False Then
            Exit Sub
        End If

        If IsInserted_Sati(datum, employeeID) And selVrstaPlacanja <> "DELETE" Then
            Exit Sub
        End If

        If selVrstaPlacanja = "P20" Then
            If IsGODaniRaspolozivo_Sati(employeeID) <= 0 Then
                Exit Sub
            End If
        End If

        ' Dodaj u bazu evidenciju za ovaj dan
        cnData.ConnectionString = ConnectionString
        cnData.Open()
        Dim insSQL As String = "INSERT INTO evd_prisustva (employeeID, sifra_placanja, datum, vrijeme_od, vrijeme_do, vrijeme_ukupno)	VALUES(" & employeeID.ToString & ", '" & selVrstaPlacanja & "','" & datum & "', '08:00:00', '16:00:00', '08:00:00');"
        Dim delSQL As String = "DELETE FROM evd_prisustva WHERE datum = '" & datum & "' AND employeeID=" & employeeID.ToString & ";"
        lcmd = New MySql.Data.MySqlClient.MySqlCommand(delSQL, cnData)
        ' lcmd.CommandText = insSQL
        Try
            If selVrstaPlacanja = "DELETE" Then
                lcmd.ExecuteNonQuery()
            Else
                'lcmd.ExecuteNonQuery()
                lcmd.CommandText = insSQL
                lcmd.ExecuteNonQuery()
            End If
        Catch ex As Exception

        End Try

        cnData.Close()
    End Sub

    Public Shared Sub InsertBezBrisanjaByRukovodilac_Sati(ByVal datum As Date, ByVal employeeID As Integer, ByVal selVrstaPlacanja As String, ByVal employeeRukovodilac As Integer)
        Dim cnData As New MySql.Data.MySqlClient.MySqlConnection
        Dim lcmd As MySql.Data.MySqlClient.MySqlCommand

        If IsOdobrenoBezUposlenika_Sati(datum, employeeID) Then
            Exit Sub
        End If

        If selVrstaPlacanja.Length < 3 Then
            Exit Sub
        End If

        If IsInserted_Sati(datum, employeeID) Then
            Exit Sub
        End If

        If selVrstaPlacanja = "P20" Then
            If IsGODaniRaspolozivo_Sati(employeeID) <= 0 Then
                Exit Sub
            End If
        End If

        'Dim cDatum As String() = datum.Split(New Char() {"/"c})
        '' Odobri evidencije prethodno unijete - a koje nisu odobrene
        'Odobrenje_EvidencijaByRole(CInt(cDatum(0)), CInt(cDatum(1)), employeeID, employeeRukovodilac, eRoles.uposlenik.ToString)
        Odobrenje_EvidencijaByRole(datum.Year, datum.Month, employeeID, employeeRukovodilac, eRoles.uposlenik.ToString)

        ' Dodaj u bazu evidenciju za ovaj dan
        cnData.ConnectionString = ConnectionString
        cnData.Open()
        Dim insSQL As String = "INSERT INTO evd_prisustva (employeeID, sifra_placanja, datum, vrijeme_ukupno, evd_podnio)	VALUES(" & employeeID.ToString & ", '" & selVrstaPlacanja & "','" & datum & "','08:00:00'," & employeeRukovodilac.ToString & ");"
        Dim delSQL As String = "DELETE FROM evd_prisustva WHERE datum = '" & datum & "' AND employeeID=" & employeeID.ToString & ";"
        lcmd = New MySql.Data.MySqlClient.MySqlCommand(delSQL, cnData)
        ' lcmd.CommandText = insSQL
        Try
            If selVrstaPlacanja = "DELETE" Then
                lcmd.ExecuteNonQuery()
            Else
                lcmd.ExecuteNonQuery()
                lcmd.CommandText = insSQL
                lcmd.ExecuteNonQuery()
            End If
        Catch ex As Exception

        End Try

        cnData.Close()
    End Sub

    Public Shared Sub InsertBezBrisanjaByUprava_Sati(ByVal datum As Date, ByVal employeeID As Integer, ByVal selVrstaPlacanja As String, ByVal employeeRukovodilac As Integer)
        Dim cnData As New MySql.Data.MySqlClient.MySqlConnection
        Dim lcmd As MySql.Data.MySqlClient.MySqlCommand

        If IsOdobrenoBezUposlenika_Sati(datum, employeeID) Then
            Exit Sub
        End If

        If selVrstaPlacanja.Length < 3 Then
            Exit Sub
        End If

        If IsInserted_Sati(datum, employeeID) Then
            Exit Sub
        End If

        If selVrstaPlacanja = "P20" Then
            If IsGODaniRaspolozivo_Sati(employeeID) <= 0 Then
                Exit Sub
            End If
        End If

        'Dim cDatum As String() = datum.Split(New Char() {"/"c})
        '' Odobri evidencije prethodno unijete - a koje nisu odobrene
        'Odobrenje_EvidencijaByRole(CInt(cDatum(0)), CInt(cDatum(1)), employeeID, employeeRukovodilac, eRoles.uposlenik.ToString)
        Odobrenje_EvidencijaByRole(datum.Year, datum.Month, employeeID, employeeRukovodilac, eRoles.uposlenik.ToString)


        ' Dodaj u bazu evidenciju za ovaj dan
        cnData.ConnectionString = ConnectionString
        cnData.Open()
        Dim insSQL As String = "INSERT INTO evd_prisustva (employeeID, sifra_placanja, datum, vrijeme_ukupno, evd_podnio)	VALUES(" & employeeID.ToString & ", '" & selVrstaPlacanja & "','" & datum & "','08:00:00'," & employeeRukovodilac.ToString & ");"
        Dim delSQL As String = "DELETE FROM evd_prisustva WHERE datum = '" & datum & "' AND employeeID=" & employeeID.ToString & ";"
        lcmd = New MySql.Data.MySqlClient.MySqlCommand(delSQL, cnData)
        ' lcmd.CommandText = insSQL
        Try
            If selVrstaPlacanja = "DELETE" Then
                lcmd.ExecuteNonQuery()
            Else
                lcmd.ExecuteNonQuery()
                lcmd.CommandText = insSQL
                lcmd.ExecuteNonQuery()
            End If
        Catch ex As Exception

        End Try

        cnData.Close()
    End Sub

    Public Shared Sub InsertMjesec_Sati(ByVal EmployeeID As Integer, ByVal selVrstaPlacanja As String, ByVal currentMonth As Integer, ByVal currentYear As Integer)

        Dim i As Integer
        For i = 1 To System.DateTime.DaysInMonth(currentYear, currentMonth)

            Dim dt As DateTime = New DateTime(currentYear, currentMonth, i)

            If dt.DayOfWeek = DayOfWeek.Saturday Or dt.DayOfWeek = DayOfWeek.Sunday Then
            Else
                ' Dodaj u kalendar
                Evidencija.InsertBezBrisanja_Sati(Evidencija.Date_MySQLFormat(dt), EmployeeID, selVrstaPlacanja)
            End If
        Next
    End Sub

    Public Shared Sub InsertByRukovodilacMjesec_Sati(ByVal EmployeeID As Integer, ByVal selVrstaPlacanja As String, ByVal currentMonth As Integer, ByVal currentYear As Integer, ByVal employeeRukovodilac As Integer)

        Dim i As Integer
        For i = 1 To System.DateTime.DaysInMonth(currentYear, currentMonth)

            Dim dt As DateTime = New DateTime(currentYear, currentMonth, i)

            If dt.DayOfWeek = DayOfWeek.Saturday Or dt.DayOfWeek = DayOfWeek.Sunday Then
            Else
                ' Dodaj u kalendar
                Evidencija.InsertBezBrisanjaByRukovodilac_Sati(dt.Date, EmployeeID, selVrstaPlacanja, employeeRukovodilac)
            End If
        Next
    End Sub

    Public Shared Sub InsertByUpravaMjesec_Sati(ByVal EmployeeID As Integer, ByVal selVrstaPlacanja As String, ByVal currentMonth As Integer, ByVal currentYear As Integer, ByVal employeeRukovodilac As Integer)

        Dim i As Integer
        For i = 1 To System.DateTime.DaysInMonth(currentYear, currentMonth)

            Dim dt As DateTime = New DateTime(currentYear, currentMonth, i)

            If dt.DayOfWeek = DayOfWeek.Saturday Or dt.DayOfWeek = DayOfWeek.Sunday Then
            Else
                ' Dodaj u kalendar
                Evidencija.InsertBezBrisanjaByUprava_Sati(dt.Date, EmployeeID, selVrstaPlacanja, employeeRukovodilac)
            End If
        Next
    End Sub

    Public Shared Sub InsertSluzbeniPut_Sati(ByVal EmployeeID As Integer, ByVal dtStart As DateTime, ByVal dtDays As Integer)
        Dim selVrstaPlacanja As String = "P06"

        Dim currentYear As Integer = dtStart.Year
        Dim currentMonth As Integer = dtStart.Month
        Dim currentDay As Integer = dtStart.Day

        Dim i As Integer
        For i = currentDay To currentDay + dtDays - 1

            Dim dt As DateTime = New DateTime(currentYear, currentMonth, i)

            If dt.DayOfWeek = DayOfWeek.Saturday Or dt.DayOfWeek = DayOfWeek.Sunday Then
            Else
                ' Dodaj u kalendar
                Evidencija.Insert_Sati(Evidencija.Date_MySQLFormat(dt), EmployeeID, selVrstaPlacanja)
            End If
        Next
    End Sub

    Public Shared Sub InsertSluzbeniPut(ByVal putniNalog As Integer)

        Dim cnData As New MySql.Data.MySqlClient.MySqlConnection
        Dim lcmd As MySql.Data.MySqlClient.MySqlCommand
        Dim ldr As MySql.Data.MySqlClient.MySqlDataReader

        Dim EmployeeID As Integer
        Dim DatPocPutovanja As DateTime
        Dim TrajanjePutovanja As Integer

        cnData.ConnectionString = ConnectionString
        cnData.Open()
        Dim strSQL As String
        Dim tmpDatum As MySql.Data.Types.MySqlDateTime

        strSQL = "SELECT putninalog_id, employee_id, dat_poc_putovanja, trajanje_putovanja FROM putninalog WHERE  putninalog_id=" & putniNalog.ToString & ";"
        lcmd = New MySql.Data.MySqlClient.MySqlCommand(strSQL, cnData)

        ldr = lcmd.ExecuteReader
        While ldr.Read
            Try
                EmployeeID = ldr.Item("employee_id")
                TrajanjePutovanja = ldr.Item("trajanje_putovanja")
            Catch ex As Exception
            End Try

            Try
                tmpDatum = CheckDBNull(ldr.Item("dat_poc_putovanja"))
                DatPocPutovanja = New DateTime(tmpDatum.Year, tmpDatum.Month, tmpDatum.Day)
            Catch ex As Exception

            End Try
        End While
        ldr.Close()
        cnData.Close()

        If IsOdobreno_Sati(Evidencija.Date_MySQLFormat(DatPocPutovanja), EmployeeID) = True Then

        Else
            InsertSluzbeniPut_Sati(EmployeeID, DatPocPutovanja, TrajanjePutovanja)
        End If


    End Sub

    Public Shared Function IsInsertDozvoljen_Sati(ByVal datum As String, ByVal employeeID As Integer, ByVal selVrstaPlacanja As String) As Boolean

        If IsOdobreno_Sati(datum, employeeID) Then
            Return False
        End If

        If selVrstaPlacanja.Length < 2 Then
            Return False
        End If

        Return True

    End Function

#End Region


    Public Shared Sub Delete_Sati(ByVal datum As String, ByVal employeeID As Integer, ByVal selVrstaPlacanja As String)
        Dim cnData As New MySql.Data.MySqlClient.MySqlConnection
        Dim lcmd As MySql.Data.MySqlClient.MySqlCommand

        If IsOdobreno_Sati(datum, employeeID) Then
            Exit Sub
        End If

        If selVrstaPlacanja.Length < 3 Then
            Exit Sub
        End If

        ' Dodaj u bazu evidenciju za ovaj dan
        cnData.ConnectionString = ConnectionString
        cnData.Open()
        Dim delSQL As String = "DELETE FROM evd_prisustva WHERE datum = '" & datum & "' AND employeeID=" & employeeID.ToString & ";"
        lcmd = New MySql.Data.MySqlClient.MySqlCommand(delSQL, cnData)
        Try
            If selVrstaPlacanja = "DELETE" Then
                lcmd.ExecuteNonQuery()
            Else
                lcmd.ExecuteNonQuery()
            End If
        Catch ex As Exception

        End Try

        cnData.Close()
    End Sub

    Public Shared Sub DeleteSluzbeniPut_Sati(ByVal EmployeeID As Integer, ByVal dtStart As DateTime, ByVal dtDays As Integer)
        Dim selVrstaPlacanja As String = "P06"

        Dim currentYear As Integer = dtStart.Year
        Dim currentMonth As Integer = dtStart.Month
        Dim currentDay As Integer = dtStart.Day

        Dim i As Integer
        For i = currentDay To currentDay + dtDays - 1

            Dim dt As DateTime = New DateTime(currentYear, currentMonth, i)

            If dt.DayOfWeek = DayOfWeek.Saturday Or dt.DayOfWeek = DayOfWeek.Sunday Then
            Else
                ' Dodaj u kalendar
                Evidencija.Delete_Sati(Evidencija.Date_MySQLFormat(dt), EmployeeID, selVrstaPlacanja)
            End If
        Next
    End Sub

    Public Shared Sub DeleteSluzbeniPut(ByVal putniNalog As Integer)

        Dim cnData As New MySql.Data.MySqlClient.MySqlConnection
        Dim lcmd As MySql.Data.MySqlClient.MySqlCommand
        Dim ldr As MySql.Data.MySqlClient.MySqlDataReader

        Dim EmployeeID As Integer
        Dim DatPocPutovanja As DateTime
        Dim TrajanjePutovanja As Integer

        cnData.ConnectionString = ConnectionString
        cnData.Open()
        Dim strSQL As String
        Dim tmpDatum As MySql.Data.Types.MySqlDateTime

        strSQL = "SELECT putninalog_id, employee_id, dat_poc_putovanja, trajanje_putovanja FROM putninalog WHERE  putninalog_id=" & putniNalog.ToString & ";"
        lcmd = New MySql.Data.MySqlClient.MySqlCommand(strSQL, cnData)

        ldr = lcmd.ExecuteReader
        While ldr.Read
            Try
                EmployeeID = ldr.Item("employee_id")
                TrajanjePutovanja = ldr.Item("trajanje_putovanja")
            Catch ex As Exception
            End Try

            Try
                tmpDatum = CheckDBNull(ldr.Item("dat_poc_putovanja"))
                DatPocPutovanja = New DateTime(tmpDatum.Year, tmpDatum.Month, tmpDatum.Day)
            Catch ex As Exception

            End Try
        End While
        ldr.Close()
        cnData.Close()

        If IsOdobreno_Sati(Evidencija.Date_MySQLFormat(DatPocPutovanja), EmployeeID) = True Then

        Else
            DeleteSluzbeniPut_Sati(EmployeeID, DatPocPutovanja, TrajanjePutovanja)
        End If


    End Sub

    Public Shared Function IsOdobreno_Sati(ByVal datum As String, ByVal employeeID As Integer) As Boolean
        Dim cnData As New MySql.Data.MySqlClient.MySqlConnection
        Dim lcmd As MySql.Data.MySqlClient.MySqlCommand
        Dim ldr As MySql.Data.MySqlClient.MySqlDataReader
        Dim retValues As Boolean = False


        cnData.ConnectionString = ConnectionString
        cnData.Open()
        ' Provjera da li evidencija potice od GO zahtjeva
        Dim selSQL As String = "SELECT * FROM evd_prisustva WHERE employeeID=" & employeeID.ToString & " AND DAY(datum)=DAY('" & datum & "') AND MONTH(datum)=MONTH('" & datum & "') AND YEAR(datum)=YEAR('" & datum & "') " _
        & "AND (evd_podnio IS NOT NULL OR evd_odobrio IS NOT NULL OR evd_kontrolisao IS NOT NULL OR evd_locked <> 0 OR evd_zahtjev_id IS NOT NULL);"

        lcmd = New MySql.Data.MySqlClient.MySqlCommand(selSQL, cnData)
        ' lcmd.CommandText = insSQL
        Try
            ldr = lcmd.ExecuteReader
            If ldr.HasRows Then
                retValues = True
            End If
            ldr.Close()
        Catch ex As Exception

        End Try

        ' Provjera da li je  evidencija odobrena
        selSQL = "SELECT * FROM evd_prisustva WHERE employeeID=" & employeeID.ToString & " AND MONTH(datum)=MONTH('" & datum & "') AND YEAR(datum)=YEAR('" & datum & "') " _
        & "AND (evd_podnio IS NOT NULL OR evd_odobrio IS NOT NULL OR evd_kontrolisao IS NOT NULL OR evd_locked <> 0);"

        lcmd = New MySql.Data.MySqlClient.MySqlCommand(selSQL, cnData)
        ' lcmd.CommandText = insSQL
        Try
            ldr = lcmd.ExecuteReader
            If ldr.HasRows Then
                retValues = True
            End If
            ldr.Close()
        Catch ex As Exception

        End Try

        cnData.Close()
        Return retValues
    End Function

    Public Shared Function IsOdobrenoBezUposlenika_Sati(ByVal datum As String, ByVal employeeID As Integer) As Boolean
        Dim cnData As New MySql.Data.MySqlClient.MySqlConnection
        Dim lcmd As MySql.Data.MySqlClient.MySqlCommand
        Dim ldr As MySql.Data.MySqlClient.MySqlDataReader
        Dim retValues As Boolean = False


        cnData.ConnectionString = ConnectionString
        cnData.Open()
        ' Provjera da li evidencija potice od GO zahtjeva
        Dim selSQL As String = "SELECT * FROM evd_prisustva WHERE employeeID=" & employeeID.ToString & " AND DAY(datum)=DAY('" & datum & "') AND MONTH(datum)=MONTH('" & datum & "') AND YEAR(datum)=YEAR('" & datum & "') " _
        & "AND (evd_odobrio IS NOT NULL OR evd_kontrolisao IS NOT NULL OR evd_locked <> 0 OR evd_zahtjev_id IS NOT NULL);"

        lcmd = New MySql.Data.MySqlClient.MySqlCommand(selSQL, cnData)
        ' lcmd.CommandText = insSQL
        Try
            ldr = lcmd.ExecuteReader
            If ldr.HasRows Then
                retValues = True
            End If
            ldr.Close()
        Catch ex As Exception

        End Try

        ' Provjera da li je  evidencija odobrena
        selSQL = "SELECT * FROM evd_prisustva WHERE employeeID=" & employeeID.ToString & " AND MONTH(datum)=MONTH('" & datum & "') AND YEAR(datum)=YEAR('" & datum & "') " _
        & "AND (evd_odobrio IS NOT NULL OR evd_kontrolisao IS NOT NULL OR evd_locked <> 0);"

        lcmd = New MySql.Data.MySqlClient.MySqlCommand(selSQL, cnData)
        ' lcmd.CommandText = insSQL
        Try
            ldr = lcmd.ExecuteReader
            If ldr.HasRows Then
                retValues = True
            End If
            ldr.Close()
        Catch ex As Exception

        End Try

        cnData.Close()
        Return retValues
    End Function

    Public Shared Function IsOdobreno_SifraPlacanja(ByVal sifraPlacanja As String) As Boolean
        Dim retValues As Boolean = False

        Select Case sifraPlacanja
            Case "P20", "P51", "P52", "P53", "P56"
                retValues = True
            Case Else

        End Select


        Return retValues
    End Function

    Public Shared Function IsOdobrenoBezUposlenikaIRukovodioca_Sati(ByVal datum As String, ByVal employeeID As Integer) As Boolean
        Dim cnData As New MySql.Data.MySqlClient.MySqlConnection
        Dim lcmd As MySql.Data.MySqlClient.MySqlCommand
        Dim ldr As MySql.Data.MySqlClient.MySqlDataReader
        Dim retValues As Boolean = False


        cnData.ConnectionString = ConnectionString
        cnData.Open()
        ' Provjera da li evidencija potice od GO zahtjeva
        Dim selSQL As String = "SELECT * FROM evd_prisustva WHERE employeeID=" & employeeID.ToString & " AND DAY(datum)=DAY('" & datum & "') AND MONTH(datum)=MONTH('" & datum & "') AND YEAR(datum)=YEAR('" & datum & "') " _
        & "AND (evd_kontrolisao IS NOT NULL OR evd_locked <> 0 OR evd_zahtjev_id IS NOT NULL);"

        lcmd = New MySql.Data.MySqlClient.MySqlCommand(selSQL, cnData)
        ' lcmd.CommandText = insSQL
        Try
            ldr = lcmd.ExecuteReader
            If ldr.HasRows Then
                retValues = True
            End If
            ldr.Close()
        Catch ex As Exception

        End Try

        ' Provjera da li je  evidencija odobrena
        selSQL = "SELECT * FROM evd_prisustva WHERE employeeID=" & employeeID.ToString & " AND MONTH(datum)=MONTH('" & datum & "') AND YEAR(datum)=YEAR('" & datum & "') " _
        & "AND (evd_kontrolisao IS NOT NULL OR evd_locked <> 0);"

        lcmd = New MySql.Data.MySqlClient.MySqlCommand(selSQL, cnData)
        ' lcmd.CommandText = insSQL
        Try
            ldr = lcmd.ExecuteReader
            If ldr.HasRows Then
                retValues = True
            End If
            ldr.Close()
        Catch ex As Exception

        End Try

        cnData.Close()
        Return retValues
    End Function

    Public Shared Function IsInserted_Sati(ByVal datum As String, ByVal employeeID As Integer) As Boolean
        Dim cnData As New MySql.Data.MySqlClient.MySqlConnection
        Dim lcmd As MySql.Data.MySqlClient.MySqlCommand
        Dim ldr As MySql.Data.MySqlClient.MySqlDataReader
        Dim retValues As Boolean = False


        cnData.ConnectionString = ConnectionString
        cnData.Open()
        ' Provjera da li evidencija potice od GO zahtjeva
        Dim selSQL As String = "SELECT * FROM evd_prisustva WHERE employeeID=" & employeeID.ToString & " AND DAY(datum)=DAY('" & datum & "') AND MONTH(datum)=MONTH('" & datum & "') AND YEAR(datum)=YEAR('" & datum & "') " _
        & ";"

        lcmd = New MySql.Data.MySqlClient.MySqlCommand(selSQL, cnData)
        ' lcmd.CommandText = insSQL
        Try
            ldr = lcmd.ExecuteReader
            If ldr.HasRows Then
                retValues = True
            End If
            ldr.Close()
        Catch ex As Exception

        End Try

        cnData.Close()
        Return retValues
    End Function

    Public Shared Sub Uposlenici_Lista(ByVal EmployeeID As Integer, ByRef employeesLista As ArrayList, _
                               ByRef clsMySqlConnection As MySql.Data.MySqlClient.MySqlConnection)

        Dim lcmd As MySql.Data.MySqlClient.MySqlCommand
        Dim ldr As MySql.Data.MySqlClient.MySqlDataReader
        Dim strSQL As String

        ' Lista ako uposlenik ima rolu Evidencije
        If OdodbrenjeIsKontrola_Evidencija(EmployeeID) Then
            strSQL = "SELECT 	EmployeeID, Concat(LastName,' ',FirstName) As Name, DepartmentUP FROM evd_employees WHERE  Aktivan <>0 Order By LastName;"
            lcmd = New MySql.Data.MySqlClient.MySqlCommand(strSQL, clsMySqlConnection)

            ldr = lcmd.ExecuteReader
            While ldr.Read
                Try
                    employeesLista.Add(New EvidencijaEmployees(ldr.Item("EmployeeID"), 3, ldr.Item("Name")))
                Catch ex As Exception
                End Try
            End While
            ldr.Close()

        Else

            strSQL = "SELECT EmployeeID, Concat(LastName,' ',FirstName) As Name, DepartmentUP 	FROM evd_employees WHERE DepartmentUP IN(" & ClsUposleniPodaci.GetEmployees_SviDepartmentUP(EmployeeID, clsMySqlConnection) & ")	AND Aktivan <>0 Order By LastName;"

            lcmd = New MySql.Data.MySqlClient.MySqlCommand(strSQL, clsMySqlConnection)
            ldr = lcmd.ExecuteReader
            While ldr.Read
                Try
                    employeesLista.Add(New EvidencijaEmployees(ldr.Item("EmployeeID"), 1, ldr.Item("Name")))
                Catch ex As Exception
                End Try
            End While
            ldr.Close()

            lcmd.CommandText = "SELECT 	EmployeeID, Concat(LastName,' ',FirstName) As Name, DepartmentUP FROM evd_employees WHERE DepartmentUP IN(SELECT id FROM sfr_organizacija WHERE `Sifra Nadnivo` IN(SELECT Sifra FROM sfr_organizacija WHERE Rukovodilac=" & EmployeeID & ")) AND Aktivan <>0 Order By LastName;"
            ldr = lcmd.ExecuteReader
            While ldr.Read
                Try
                    employeesLista.Add(New EvidencijaEmployees(ldr.Item("EmployeeID"), 2, ldr.Item("Name")))
                Catch ex As Exception
                End Try
            End While
            ldr.Close()

            lcmd.CommandText = "SELECT 	EmployeeID, Concat(LastName,' ',FirstName) As Name, DepartmentUP FROM evd_employees WHERE DepartmentUP IN(SELECT id FROM sfr_organizacija WHERE `Sifra Nadnivo` IN(SELECT Sifra FROM sfr_organizacija WHERE `Sifra Nadnivo` IN(SELECT Sifra FROM sfr_organizacija WHERE Rukovodilac=" & EmployeeID & " ))) AND Aktivan <>0 Order By LastName;"
            ldr = lcmd.ExecuteReader
            While ldr.Read
                Try
                    employeesLista.Add(New EvidencijaEmployees(ldr.Item("EmployeeID"), 3, ldr.Item("Name")))
                Catch ex As Exception
                End Try
            End While

            ldr.Close()

        End If


    End Sub

    Public Shared Sub Uposlenici_ListaByRole(ByVal RoleName As String, ByRef employeesLista As ArrayList, _
                                Optional ByVal EmployeeID As Integer = -1)

        Dim lcmd As MySql.Data.MySqlClient.MySqlCommand
        Dim ldr As MySql.Data.MySqlClient.MySqlDataReader
        Dim strSQL As String


        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)

            myconnection.Open()

            Select Case RoleName

                Case "pravna"
                    strSQL = "SELECT EmployeeID, Concat(LastName,' ',FirstName) As Name, DepartmentUP FROM evd_employees WHERE  Aktivan <>0 Order By LastName;"
                    lcmd = New MySql.Data.MySqlClient.MySqlCommand(strSQL, myconnection)
                    Try
                        ldr = lcmd.ExecuteReader
                        While ldr.Read
                            Try
                                employeesLista.Add(New EvidencijaEmployees(ldr.Item("EmployeeID"), 3, ldr.Item("Name")))
                            Catch ex As Exception
                            End Try
                        End While
                        ldr.Close()
                    Catch ex As Exception

                    End Try


                Case "evidencija"
                    strSQL = "SELECT EmployeeID, Concat(LastName,' ',FirstName) As Name, DepartmentUP FROM evd_employees WHERE  Aktivan <>0 Order By LastName;"
                    lcmd = New MySql.Data.MySqlClient.MySqlCommand(strSQL, myconnection)

                    Try
                        ldr = lcmd.ExecuteReader
                        While ldr.Read
                            Try
                                employeesLista.Add(New EvidencijaEmployees(ldr.Item("EmployeeID"), 3, ldr.Item("Name")))
                            Catch ex As Exception
                            End Try
                        End While
                        ldr.Close()
                    Catch ex As Exception

                    End Try


                Case "rukovodilac"
                    strSQL = "SELECT EmployeeID, Concat(LastName,' ',FirstName) As Name, DepartmentUP 	FROM evd_employees WHERE DepartmentUP IN(" & ClsUposleniPodaci.GetEmployees_SviDepartmentUP(EmployeeID, myconnection) & ")	AND Aktivan <>0 AND EmployeeID <> " & EmployeeID & " Order By LastName;"
                    lcmd = New MySql.Data.MySqlClient.MySqlCommand(strSQL, myconnection)

                    Try
                        ldr = lcmd.ExecuteReader
                        While ldr.Read
                            Try
                                employeesLista.Add(New EvidencijaEmployees(ldr.Item("EmployeeID"), 3, ldr.Item("Name")))
                            Catch ex As Exception
                            End Try
                        End While
                        ldr.Close()
                    Catch ex As Exception

                    End Try




                Case "uprava"
                    strSQL = "SELECT EmployeeID, Concat(LastName,' ',FirstName) As Name, DepartmentUP 	FROM evd_employees WHERE DepartmentUp = 5 OR EmployeeID IN (SELECT e.EmployeeID FROM my_aspnet_membership AS u INNER JOIN my_aspnet_users_to_employees AS u2e " _
                & " ON (u.userId = u2e.users_id) INNER JOIN evd_employees AS e ON (e.EmployeeID = u2e.employees_id) " _
                & " INNER JOIN my_aspnet_sistematizacija_to_roles AS s2r ON (e.RadnoMjesto = s2r.sistematizacija_id) WHERE s2r.roles_id=1) AND Aktivan <>0 Order By LastName;"
                    lcmd = New MySql.Data.MySqlClient.MySqlCommand(strSQL, myconnection)

                    Try
                        ldr = lcmd.ExecuteReader
                        While ldr.Read
                            Try
                                employeesLista.Add(New EvidencijaEmployees(ldr.Item("EmployeeID"), 3, ldr.Item("Name")))
                            Catch ex As Exception
                            End Try
                        End While
                        ldr.Close()
                    Catch ex As Exception

                    End Try



                Case Else
                    strSQL = "SELECT EmployeeID, Concat(LastName,' ',FirstName) As Name, DepartmentUP FROM evd_employees WHERE  Aktivan <>0 AND EmployeeID = " & EmployeeID & " Order By LastName;"
                    lcmd = New MySql.Data.MySqlClient.MySqlCommand(strSQL, myconnection)

                    Try
                        ldr = lcmd.ExecuteReader
                        While ldr.Read
                            Try
                                employeesLista.Add(New EvidencijaEmployees(ldr.Item("EmployeeID"), 3, ldr.Item("Name")))
                            Catch ex As Exception
                            End Try
                        End While
                        ldr.Close()
                    Catch ex As Exception

                    End Try

            End Select

        End Using



    End Sub

    Public Shared Function IsGODaniRaspolozivo_Sati(ByVal employeeID As Integer) As Integer
        Dim cnData As New MySql.Data.MySqlClient.MySqlConnection
        Dim lcmd As MySql.Data.MySqlClient.MySqlCommand
        Dim ldr As MySql.Data.MySqlClient.MySqlDataReader
        Dim retValues As Boolean = False
        Dim goGodina As Integer
        Dim goGodina_Dana As Integer = 0

        cnData.ConnectionString = ConnectionString
        cnData.Open()
        ' Provjera da li evidencija potice od GO zahtjeva
        Dim selSQL As String = "SELECT Godina FROM evd_godisnjiodmor WHERE employeeID = " & employeeID.ToString & " AND rjesenje_status <> 0 ORDER BY Godina DESC LIMIT 0,1;"

        lcmd = New MySql.Data.MySqlClient.MySqlCommand(selSQL, cnData)
        ' lcmd.CommandText = insSQL
        Try
            ldr = lcmd.ExecuteReader
            If ldr.HasRows Then
                While ldr.Read
                    goGodina = ldr.Item("Godina")
                End While
            End If
            ldr.Close()
        Catch ex As Exception

        End Try

        'selSQL = "SELECT e.employeeID, COUNT(e.sifra_placanja) AS bdgo, YEAR(e.datum) AS yr, ego.IzrDani + ego.NeIskDani - COUNT(e.sifra_placanja) AS GoDani " _
        '    & " FROM evd_prisustva e, evd_godisnjiodmor ego " _
        '    & " WHERE e.sifra_placanja = 'P20' AND e.datum >= ego.datum AND e.employeeID=ego.employeeID AND ego.Godina = " & goGodina.ToString & " " _
        '    & " AND YEAR(e.datum) = " & goGodina.ToString & " AND e.employeeID = " & employeeID.ToString & " GROUP BY e.employeeID, YEAR(e.datum) " _
        '    & " ORDER BY e.employeeID, YEAR(e.datum), MONTH(e.datum);"

        selSQL = "SELECT ego.employeeID,  ego.IzrDani + ego.NeIskDani  AS GoDani FROM evd_godisnjiodmor ego " _
             + " WHERE ego.Godina = " & goGodina.ToString & " AND ego.employeeID = " & employeeID.ToString & ";"

        lcmd.CommandText = selSQL

        Try
            ldr = lcmd.ExecuteReader
            If ldr.HasRows Then
                While ldr.Read
                    goGodina_Dana = ldr.Item("GoDani")
                End While
            End If
            ldr.Close()
        Catch ex As Exception

        End Try

        cnData.Close()
        Return goGodina_Dana
    End Function

#Region "**** Suma Sati ****"

    Public Shared Sub Suma_Sati(ByRef sumaSati As String, ByVal datum As System.DateTime, ByVal employeeID As Integer)
        Dim cnData As New MySql.Data.MySqlClient.MySqlConnection
        Dim lcmd As MySql.Data.MySqlClient.MySqlCommand
        Dim ldr As MySql.Data.MySqlClient.MySqlDataReader

        Dim sYear As String = datum.Year.ToString
        Dim sMonth As String = datum.Month.ToString
        Dim sDays As String = Date.DaysInMonth(datum.Year, datum.Month)

        Dim startDate As String = sYear & "/" & sMonth & "/01"
        Dim endDate As String = sYear & "/" & sMonth & "/" & sDays

        sumaSati = "Ukupno: 00:00"

        ' Dodaj u bazu evidenciju za ovaj dan
        cnData.ConnectionString = ConnectionString
        cnData.Open()

        Dim strSQL As String = " SELECT CONCAT(HOUR(SEC_TO_TIME( SUM( TIME_TO_SEC( vrijeme_ukupno )))),':',MINUTE(SEC_TO_TIME( SUM( TIME_TO_SEC( vrijeme_ukupno )))))  AS ukupno " _
                        & " FROM evd_prisustva  WHERE employeeID=" & employeeID & " AND datum >= '" & startDate & "' AND datum <= '" & endDate & "';"

        lcmd = New MySql.Data.MySqlClient.MySqlCommand(strSQL, cnData)
        ldr = lcmd.ExecuteReader
        Try
            While ldr.Read
                Try
                    sumaSati = "Ukupno: " & MySQL_ByteToString(ldr.Item("ukupno"))
                Catch ex As Exception
                    sumaSati = "Ukupno: 00:00"
                End Try
            End While
        Catch ex As Exception

        End Try

        ldr.Close()
        cnData.Close()

    End Sub

    Public Shared Sub Suma_SatiBol(ByRef sumaSati As String, ByVal datum As System.DateTime, ByVal employeeID As Integer)
        Dim cnData As New MySql.Data.MySqlClient.MySqlConnection
        Dim lcmd As MySql.Data.MySqlClient.MySqlCommand
        Dim ldr As MySql.Data.MySqlClient.MySqlDataReader

        Dim sYear As String = datum.Year.ToString
        Dim sMonth As String = datum.Month.ToString
        Dim sDays As String = Date.DaysInMonth(datum.Year, datum.Month)

        Dim startDate As String = sYear & "/" & sMonth & "/01"
        Dim endDate As String = sYear & "/" & sMonth & "/" & sDays

        sumaSati = "Ukupno: 00:00"

        ' Dodaj u bazu evidenciju za ovaj dan
        cnData.ConnectionString = ConnectionString
        cnData.Open()

        Dim strSQL As String = " SELECT CONCAT(HOUR(SEC_TO_TIME( SUM( TIME_TO_SEC( vrijeme_ukupno )))),':',MINUTE(SEC_TO_TIME( SUM( TIME_TO_SEC( vrijeme_ukupno )))))  AS ukupno " _
                        & " FROM evd_prisustva  WHERE employeeID=" & employeeID & " AND datum >= '" & startDate & "' AND datum <= '" & endDate & "' and sifra_placanja LIKE 'P5%';"

        lcmd = New MySql.Data.MySqlClient.MySqlCommand(strSQL, cnData)
        ldr = lcmd.ExecuteReader
        Try
            While ldr.Read
                Try
                    sumaSati = "Ukupno bolovanje: " & MySQL_ByteToString(ldr.Item("ukupno"))
                Catch ex As Exception
                    sumaSati = "Ukupno bolovanje: 00:00"
                End Try
            End While
        Catch ex As Exception

        End Try

        ldr.Close()
        cnData.Close()

    End Sub

    Public Shared Sub Suma_Sati_Plac(ByRef sumaSati As String, ByVal datum As System.DateTime, ByVal employeeID As Integer, ByVal vrstaPlac As String)
        Dim cnData As New MySql.Data.MySqlClient.MySqlConnection
        Dim lcmd As MySql.Data.MySqlClient.MySqlCommand
        Dim ldr As MySql.Data.MySqlClient.MySqlDataReader

        Dim sYear As String = datum.Year.ToString
        Dim sMonth As String = datum.Month.ToString
        Dim sDays As String = Date.DaysInMonth(datum.Year, datum.Month)

        Dim startDate As String = sYear & "/" & sMonth & "/01"
        Dim endDate As String = sYear & "/" & sMonth & "/" & sDays

        sumaSati = ""

        ' Dodaj u bazu evidenciju za ovaj dan
        cnData.ConnectionString = ConnectionString
        cnData.Open()

        Dim strSQL As String = " SELECT CONCAT(HOUR(SEC_TO_TIME( SUM( TIME_TO_SEC( vrijeme_ukupno )))),':',MINUTE(SEC_TO_TIME( SUM( TIME_TO_SEC( vrijeme_ukupno )))))  AS ukupno " _
                        & " FROM evd_prisustva  WHERE employeeID=" & employeeID & " AND datum >= '" & startDate & "' AND datum <= '" & endDate & "' AND sifra_placanja = '" + vrstaPlac + "';"

        lcmd = New MySql.Data.MySqlClient.MySqlCommand(strSQL, cnData)
        ldr = lcmd.ExecuteReader
        Try
            While ldr.Read
                Try
                    sumaSati = MySQL_ByteToString(ldr.Item("ukupno"))
                Catch ex As Exception
                    sumaSati = ""
                End Try
            End While
        Catch ex As Exception

        End Try

        ldr.Close()
        cnData.Close()

    End Sub

    Public Shared Sub Suma_Sati_Plac_RR(ByRef sumaSati As String, ByVal datum As System.DateTime, ByVal employeeID As Integer, ByVal vrstaPlac As String)
        Dim cnData As New MySql.Data.MySqlClient.MySqlConnection
        Dim lcmd As MySql.Data.MySqlClient.MySqlCommand
        Dim ldr As MySql.Data.MySqlClient.MySqlDataReader

        Dim sYear As String = datum.Year.ToString
        Dim sMonth As String = datum.Month.ToString
        Dim sDays As String = Date.DaysInMonth(datum.Year, datum.Month)

        Dim startDate As String = sYear & "/" & sMonth & "/01"
        Dim endDate As String = sYear & "/" & sMonth & "/" & sDays

        sumaSati = ""

        ' Dodaj u bazu evidenciju za ovaj dan
        cnData.ConnectionString = ConnectionString
        cnData.Open()

        Dim strSQL As String = " SELECT CONCAT(HOUR(SEC_TO_TIME( SUM( TIME_TO_SEC( vrijeme_ukupno )))),':',MINUTE(SEC_TO_TIME( SUM( TIME_TO_SEC( vrijeme_ukupno )))))  AS ukupno " _
                        & " FROM evd_prisustva  WHERE employeeID=" & employeeID & " AND datum >= '" & startDate & "' AND datum <= '" & endDate & "' AND sifra_placanja LIKE 'RR%';"

        lcmd = New MySql.Data.MySqlClient.MySqlCommand(strSQL, cnData)
        ldr = lcmd.ExecuteReader
        Try
            While ldr.Read
                Try
                    sumaSati = MySQL_ByteToString(ldr.Item("ukupno"))
                Catch ex As Exception
                    sumaSati = ""
                End Try
            End While
        Catch ex As Exception

        End Try

        ldr.Close()
        cnData.Close()

    End Sub

    Public Shared Sub Suma_Sati_P03(ByRef sumaSati As String, ByVal datum As System.DateTime, ByVal employeeID As Integer)
        Dim cnData As New MySql.Data.MySqlClient.MySqlConnection
        Dim lcmd As MySql.Data.MySqlClient.MySqlCommand
        Dim ldr As MySql.Data.MySqlClient.MySqlDataReader

        Dim sYear As String = datum.Year.ToString
        Dim sMonth As String = datum.Month.ToString
        Dim sDays As String = Date.DaysInMonth(datum.Year, datum.Month)

        Dim startDate As String = sYear & "/" & sMonth & "/01"
        Dim endDate As String = sYear & "/" & sMonth & "/" & sDays

        sumaSati = ""

        ' Dodaj u bazu evidenciju za ovaj dan
        cnData.ConnectionString = ConnectionString
        cnData.Open()

        Dim strSQL As String = " SELECT CONCAT(HOUR(SEC_TO_TIME( SUM( TIME_TO_SEC( vrijeme_ukupno )))),':',MINUTE(SEC_TO_TIME( SUM( TIME_TO_SEC( vrijeme_ukupno )))))  AS ukupno " _
                        & " FROM evd_prisustva  WHERE employeeID=" & employeeID & " AND datum >= '" & startDate & "' AND datum <= '" & endDate & "' AND sifra_placanja = 'P03';"

        lcmd = New MySql.Data.MySqlClient.MySqlCommand(strSQL, cnData)
        ldr = lcmd.ExecuteReader
        Try
            While ldr.Read
                Try
                    sumaSati = MySQL_ByteToString(ldr.Item("ukupno"))
                Catch ex As Exception
                    sumaSati = ""
                End Try
            End While
        Catch ex As Exception

        End Try

        ldr.Close()
        cnData.Close()

    End Sub
    Public Shared Sub Suma_Sati_P04(ByRef sumaSati As String, ByVal datum As System.DateTime, ByVal employeeID As Integer)
        Dim cnData As New MySql.Data.MySqlClient.MySqlConnection
        Dim lcmd As MySql.Data.MySqlClient.MySqlCommand
        Dim ldr As MySql.Data.MySqlClient.MySqlDataReader

        Dim sYear As String = datum.Year.ToString
        Dim sMonth As String = datum.Month.ToString
        Dim sDays As String = Date.DaysInMonth(datum.Year, datum.Month)

        Dim startDate As String = sYear & "/" & sMonth & "/01"
        Dim endDate As String = sYear & "/" & sMonth & "/" & sDays

        sumaSati = ""

        ' Dodaj u bazu evidenciju za ovaj dan
        cnData.ConnectionString = ConnectionString
        cnData.Open()

        Dim strSQL As String = " SELECT CONCAT(HOUR(SEC_TO_TIME( SUM( TIME_TO_SEC( vrijeme_ukupno )))),':',MINUTE(SEC_TO_TIME( SUM( TIME_TO_SEC( vrijeme_ukupno )))))  AS ukupno " _
                        & " FROM evd_prisustva  WHERE employeeID=" & employeeID & " AND datum >= '" & startDate & "' AND datum <= '" & endDate & "' AND sifra_placanja = 'P04';"

        lcmd = New MySql.Data.MySqlClient.MySqlCommand(strSQL, cnData)
        ldr = lcmd.ExecuteReader
        Try
            While ldr.Read
                Try
                    sumaSati = MySQL_ByteToString(ldr.Item("ukupno"))
                Catch ex As Exception
                    sumaSati = ""
                End Try
            End While
        Catch ex As Exception

        End Try

        ldr.Close()
        cnData.Close()

    End Sub
    Public Shared Sub Suma_Sati_P05(ByRef sumaSati As String, ByVal datum As System.DateTime, ByVal employeeID As Integer)
        Dim cnData As New MySql.Data.MySqlClient.MySqlConnection
        Dim lcmd As MySql.Data.MySqlClient.MySqlCommand
        Dim ldr As MySql.Data.MySqlClient.MySqlDataReader

        Dim sYear As String = datum.Year.ToString
        Dim sMonth As String = datum.Month.ToString
        Dim sDays As String = Date.DaysInMonth(datum.Year, datum.Month)

        Dim startDate As String = sYear & "/" & sMonth & "/01"
        Dim endDate As String = sYear & "/" & sMonth & "/" & sDays

        sumaSati = ""

        ' Dodaj u bazu evidenciju za ovaj dan
        cnData.ConnectionString = ConnectionString
        cnData.Open()

        Dim strSQL As String = " SELECT CONCAT(HOUR(SEC_TO_TIME( SUM( TIME_TO_SEC( vrijeme_ukupno )))),':',MINUTE(SEC_TO_TIME( SUM( TIME_TO_SEC( vrijeme_ukupno )))))  AS ukupno " _
                        & " FROM evd_prisustva  WHERE employeeID=" & employeeID & " AND datum >= '" & startDate & "' AND datum <= '" & endDate & "' AND sifra_placanja = 'P05';"

        lcmd = New MySql.Data.MySqlClient.MySqlCommand(strSQL, cnData)
        ldr = lcmd.ExecuteReader
        Try
            While ldr.Read
                Try
                    sumaSati = MySQL_ByteToString(ldr.Item("ukupno"))
                Catch ex As Exception
                    sumaSati = ""
                End Try
            End While
        Catch ex As Exception

        End Try

        ldr.Close()
        cnData.Close()

    End Sub
    Public Shared Sub Suma_Sati_P06(ByRef sumaSati As String, ByVal datum As System.DateTime, ByVal employeeID As Integer)
        Dim cnData As New MySql.Data.MySqlClient.MySqlConnection
        Dim lcmd As MySql.Data.MySqlClient.MySqlCommand
        Dim ldr As MySql.Data.MySqlClient.MySqlDataReader

        Dim sYear As String = datum.Year.ToString
        Dim sMonth As String = datum.Month.ToString
        Dim sDays As String = Date.DaysInMonth(datum.Year, datum.Month)

        Dim startDate As String = sYear & "/" & sMonth & "/01"
        Dim endDate As String = sYear & "/" & sMonth & "/" & sDays

        sumaSati = ""

        ' Dodaj u bazu evidenciju za ovaj dan
        cnData.ConnectionString = ConnectionString
        cnData.Open()

        Dim strSQL As String = " SELECT CONCAT(HOUR(SEC_TO_TIME( SUM( TIME_TO_SEC( vrijeme_ukupno )))),':',MINUTE(SEC_TO_TIME( SUM( TIME_TO_SEC( vrijeme_ukupno )))))  AS ukupno " _
                        & " FROM evd_prisustva  WHERE employeeID=" & employeeID & " AND datum >= '" & startDate & "' AND datum <= '" & endDate & "' AND sifra_placanja = 'P06';"

        lcmd = New MySql.Data.MySqlClient.MySqlCommand(strSQL, cnData)
        ldr = lcmd.ExecuteReader
        Try
            While ldr.Read
                Try
                    sumaSati = MySQL_ByteToString(ldr.Item("ukupno"))
                Catch ex As Exception
                    sumaSati = ""
                End Try
            End While
        Catch ex As Exception

        End Try

        ldr.Close()
        cnData.Close()

    End Sub
    Public Shared Sub Suma_Sati_P09(ByRef sumaSati As String, ByVal datum As System.DateTime, ByVal employeeID As Integer)
        Dim cnData As New MySql.Data.MySqlClient.MySqlConnection
        Dim lcmd As MySql.Data.MySqlClient.MySqlCommand
        Dim ldr As MySql.Data.MySqlClient.MySqlDataReader

        Dim sYear As String = datum.Year.ToString
        Dim sMonth As String = datum.Month.ToString
        Dim sDays As String = Date.DaysInMonth(datum.Year, datum.Month)

        Dim startDate As String = sYear & "/" & sMonth & "/01"
        Dim endDate As String = sYear & "/" & sMonth & "/" & sDays

        sumaSati = ""

        ' Dodaj u bazu evidenciju za ovaj dan
        cnData.ConnectionString = ConnectionString
        cnData.Open()

        Dim strSQL As String = " SELECT CONCAT(HOUR(SEC_TO_TIME( SUM( TIME_TO_SEC( vrijeme_ukupno )))),':',MINUTE(SEC_TO_TIME( SUM( TIME_TO_SEC( vrijeme_ukupno )))))  AS ukupno " _
                        & " FROM evd_prisustva  WHERE employeeID=" & employeeID & " AND datum >= '" & startDate & "' AND datum <= '" & endDate & "' AND sifra_placanja = 'P09';"

        lcmd = New MySql.Data.MySqlClient.MySqlCommand(strSQL, cnData)
        ldr = lcmd.ExecuteReader
        Try
            While ldr.Read
                Try
                    sumaSati = MySQL_ByteToString(ldr.Item("ukupno"))
                Catch ex As Exception
                    sumaSati = ""
                End Try
            End While
        Catch ex As Exception

        End Try

        ldr.Close()
        cnData.Close()

    End Sub
    Public Shared Sub Suma_Sati_P20(ByRef sumaSati As String, ByVal datum As System.DateTime, ByVal employeeID As Integer)
        Dim cnData As New MySql.Data.MySqlClient.MySqlConnection
        Dim lcmd As MySql.Data.MySqlClient.MySqlCommand
        Dim ldr As MySql.Data.MySqlClient.MySqlDataReader

        Dim sYear As String = datum.Year.ToString
        Dim sMonth As String = datum.Month.ToString
        Dim sDays As String = Date.DaysInMonth(datum.Year, datum.Month)

        Dim startDate As String = sYear & "/" & sMonth & "/01"
        Dim endDate As String = sYear & "/" & sMonth & "/" & sDays

        sumaSati = ""

        ' Dodaj u bazu evidenciju za ovaj dan
        cnData.ConnectionString = ConnectionString
        cnData.Open()

        Dim strSQL As String = " SELECT CONCAT(HOUR(SEC_TO_TIME( SUM( TIME_TO_SEC( vrijeme_ukupno )))),':',MINUTE(SEC_TO_TIME( SUM( TIME_TO_SEC( vrijeme_ukupno )))))  AS ukupno " _
                        & " FROM evd_prisustva  WHERE employeeID=" & employeeID & " AND datum >= '" & startDate & "' AND datum <= '" & endDate & "' AND sifra_placanja = 'P20';"

        lcmd = New MySql.Data.MySqlClient.MySqlCommand(strSQL, cnData)
        ldr = lcmd.ExecuteReader
        Try
            While ldr.Read
                Try
                    sumaSati = MySQL_ByteToString(ldr.Item("ukupno"))
                Catch ex As Exception
                    sumaSati = ""
                End Try
            End While
        Catch ex As Exception

        End Try

        ldr.Close()
        cnData.Close()

    End Sub
    Public Shared Sub Suma_Sati_P21(ByRef sumaSati As String, ByVal datum As System.DateTime, ByVal employeeID As Integer)
        Dim cnData As New MySql.Data.MySqlClient.MySqlConnection
        Dim lcmd As MySql.Data.MySqlClient.MySqlCommand
        Dim ldr As MySql.Data.MySqlClient.MySqlDataReader

        Dim sYear As String = datum.Year.ToString
        Dim sMonth As String = datum.Month.ToString
        Dim sDays As String = Date.DaysInMonth(datum.Year, datum.Month)

        Dim startDate As String = sYear & "/" & sMonth & "/01"
        Dim endDate As String = sYear & "/" & sMonth & "/" & sDays

        sumaSati = ""

        ' Dodaj u bazu evidenciju za ovaj dan
        cnData.ConnectionString = ConnectionString
        cnData.Open()

        Dim strSQL As String = " SELECT CONCAT(HOUR(SEC_TO_TIME( SUM( TIME_TO_SEC( vrijeme_ukupno )))),':',MINUTE(SEC_TO_TIME( SUM( TIME_TO_SEC( vrijeme_ukupno )))))  AS ukupno " _
                        & " FROM evd_prisustva  WHERE employeeID=" & employeeID & " AND datum >= '" & startDate & "' AND datum <= '" & endDate & "' AND sifra_placanja = 'P21';"

        lcmd = New MySql.Data.MySqlClient.MySqlCommand(strSQL, cnData)
        ldr = lcmd.ExecuteReader
        Try
            While ldr.Read
                Try
                    sumaSati = MySQL_ByteToString(ldr.Item("ukupno"))
                Catch ex As Exception
                    sumaSati = ""
                End Try
            End While
        Catch ex As Exception

        End Try

        ldr.Close()
        cnData.Close()

    End Sub
    Public Shared Sub Suma_Sati_P51(ByRef sumaSati As String, ByVal datum As System.DateTime, ByVal employeeID As Integer)
        Dim cnData As New MySql.Data.MySqlClient.MySqlConnection
        Dim lcmd As MySql.Data.MySqlClient.MySqlCommand
        Dim ldr As MySql.Data.MySqlClient.MySqlDataReader

        Dim sYear As String = datum.Year.ToString
        Dim sMonth As String = datum.Month.ToString
        Dim sDays As String = Date.DaysInMonth(datum.Year, datum.Month)

        Dim startDate As String = sYear & "/" & sMonth & "/01"
        Dim endDate As String = sYear & "/" & sMonth & "/" & sDays

        sumaSati = ""

        ' Dodaj u bazu evidenciju za ovaj dan
        cnData.ConnectionString = ConnectionString
        cnData.Open()

        Dim strSQL As String = " SELECT CONCAT(HOUR(SEC_TO_TIME( SUM( TIME_TO_SEC( vrijeme_ukupno )))),':',MINUTE(SEC_TO_TIME( SUM( TIME_TO_SEC( vrijeme_ukupno )))))  AS ukupno " _
                        & " FROM evd_prisustva  WHERE employeeID=" & employeeID & " AND datum >= '" & startDate & "' AND datum <= '" & endDate & "' AND sifra_placanja = 'P51';"

        lcmd = New MySql.Data.MySqlClient.MySqlCommand(strSQL, cnData)
        ldr = lcmd.ExecuteReader
        Try
            While ldr.Read
                Try
                    sumaSati = MySQL_ByteToString(ldr.Item("ukupno"))
                Catch ex As Exception
                    sumaSati = ""
                End Try
            End While
        Catch ex As Exception

        End Try

        ldr.Close()
        cnData.Close()

    End Sub
    Public Shared Sub Suma_Sati_P52(ByRef sumaSati As String, ByVal datum As System.DateTime, ByVal employeeID As Integer)
        Dim cnData As New MySql.Data.MySqlClient.MySqlConnection
        Dim lcmd As MySql.Data.MySqlClient.MySqlCommand
        Dim ldr As MySql.Data.MySqlClient.MySqlDataReader

        Dim sYear As String = datum.Year.ToString
        Dim sMonth As String = datum.Month.ToString
        Dim sDays As String = Date.DaysInMonth(datum.Year, datum.Month)

        Dim startDate As String = sYear & "/" & sMonth & "/01"
        Dim endDate As String = sYear & "/" & sMonth & "/" & sDays

        sumaSati = ""

        ' Dodaj u bazu evidenciju za ovaj dan
        cnData.ConnectionString = ConnectionString
        cnData.Open()

        Dim strSQL As String = " SELECT CONCAT(HOUR(SEC_TO_TIME( SUM( TIME_TO_SEC( vrijeme_ukupno )))),':',MINUTE(SEC_TO_TIME( SUM( TIME_TO_SEC( vrijeme_ukupno )))))  AS ukupno " _
                        & " FROM evd_prisustva  WHERE employeeID=" & employeeID & " AND datum >= '" & startDate & "' AND datum <= '" & endDate & "' AND sifra_placanja = 'P52';"

        lcmd = New MySql.Data.MySqlClient.MySqlCommand(strSQL, cnData)
        ldr = lcmd.ExecuteReader
        Try
            While ldr.Read
                Try
                    sumaSati = MySQL_ByteToString(ldr.Item("ukupno"))
                Catch ex As Exception
                    sumaSati = ""
                End Try
            End While
        Catch ex As Exception

        End Try

        ldr.Close()
        cnData.Close()

    End Sub
    Public Shared Sub Suma_Sati_P53(ByRef sumaSati As String, ByVal datum As System.DateTime, ByVal employeeID As Integer)
        Dim cnData As New MySql.Data.MySqlClient.MySqlConnection
        Dim lcmd As MySql.Data.MySqlClient.MySqlCommand
        Dim ldr As MySql.Data.MySqlClient.MySqlDataReader

        Dim sYear As String = datum.Year.ToString
        Dim sMonth As String = datum.Month.ToString
        Dim sDays As String = Date.DaysInMonth(datum.Year, datum.Month)

        Dim startDate As String = sYear & "/" & sMonth & "/01"
        Dim endDate As String = sYear & "/" & sMonth & "/" & sDays

        sumaSati = ""

        ' Dodaj u bazu evidenciju za ovaj dan
        cnData.ConnectionString = ConnectionString
        cnData.Open()

        Dim strSQL As String = " SELECT CONCAT(HOUR(SEC_TO_TIME( SUM( TIME_TO_SEC( vrijeme_ukupno )))),':',MINUTE(SEC_TO_TIME( SUM( TIME_TO_SEC( vrijeme_ukupno )))))  AS ukupno " _
                        & " FROM evd_prisustva  WHERE employeeID=" & employeeID & " AND datum >= '" & startDate & "' AND datum <= '" & endDate & "' AND sifra_placanja = 'P53';"

        lcmd = New MySql.Data.MySqlClient.MySqlCommand(strSQL, cnData)
        ldr = lcmd.ExecuteReader
        Try
            While ldr.Read
                Try
                    sumaSati = MySQL_ByteToString(ldr.Item("ukupno"))
                Catch ex As Exception
                    sumaSati = ""
                End Try
            End While
        Catch ex As Exception

        End Try

        ldr.Close()
        cnData.Close()

    End Sub
    Public Shared Sub Suma_Sati_P56(ByRef sumaSati As String, ByVal datum As System.DateTime, ByVal employeeID As Integer)
        Dim cnData As New MySql.Data.MySqlClient.MySqlConnection
        Dim lcmd As MySql.Data.MySqlClient.MySqlCommand
        Dim ldr As MySql.Data.MySqlClient.MySqlDataReader

        Dim sYear As String = datum.Year.ToString
        Dim sMonth As String = datum.Month.ToString
        Dim sDays As String = Date.DaysInMonth(datum.Year, datum.Month)

        Dim startDate As String = sYear & "/" & sMonth & "/01"
        Dim endDate As String = sYear & "/" & sMonth & "/" & sDays

        sumaSati = ""

        ' Dodaj u bazu evidenciju za ovaj dan
        cnData.ConnectionString = ConnectionString
        cnData.Open()

        Dim strSQL As String = " SELECT CONCAT(HOUR(SEC_TO_TIME( SUM( TIME_TO_SEC( vrijeme_ukupno )))),':',MINUTE(SEC_TO_TIME( SUM( TIME_TO_SEC( vrijeme_ukupno )))))  AS ukupno " _
                        & " FROM evd_prisustva  WHERE employeeID=" & employeeID & " AND datum >= '" & startDate & "' AND datum <= '" & endDate & "' AND sifra_placanja = 'P56';"

        lcmd = New MySql.Data.MySqlClient.MySqlCommand(strSQL, cnData)
        ldr = lcmd.ExecuteReader
        Try
            While ldr.Read
                Try
                    sumaSati = MySQL_ByteToString(ldr.Item("ukupno"))
                Catch ex As Exception
                    sumaSati = ""
                End Try
            End While
        Catch ex As Exception

        End Try

        ldr.Close()
        cnData.Close()

    End Sub


#End Region

    Public Shared Sub Odobrenje_Sati(ByVal datum As System.DateTime, ByVal employeeID As Integer, ByVal odobravaID As Integer)
        Dim cnData As New MySql.Data.MySqlClient.MySqlConnection
        Dim lcmd As MySql.Data.MySqlClient.MySqlCommand

        Dim sYear As String = datum.Year.ToString
        Dim sMonth As String = datum.Month.ToString
        Dim sDays As String = Date.DaysInMonth(datum.Year, datum.Month)

        Dim startDate As String = sYear & "/" & sMonth & "/01"
        Dim endDate As String = sYear & "/" & sMonth & "/" & sDays


        ' Dodaj u bazu evidenciju za ovaj dan
        cnData.ConnectionString = ConnectionString
        cnData.Open()

        Dim insSQL As String
        insSQL = "INSERT INTO evd_prisustva_odobrenja (evd_prisustva_id, employeeID)SELECT 	evd_prisustva_id, " & odobravaID & " FROM evd_prisustva WHERE employeeID = " & employeeID & " AND datum >= '" & startDate & "' AND datum <= '" & endDate & "';"
        lcmd = New MySql.Data.MySqlClient.MySqlCommand(insSQL, cnData)
        Try
            lcmd.ExecuteNonQuery()
        Catch ex As Exception

        End Try

        cnData.Close()

    End Sub

    Public Shared Sub PonistiOdobrenje_Sati(ByVal datum As System.DateTime, ByVal employeeID As Integer, ByVal odobravaID As Integer)
        Dim cnData As New MySql.Data.MySqlClient.MySqlConnection
        Dim lcmd As MySql.Data.MySqlClient.MySqlCommand

        Dim sYear As String = datum.Year.ToString
        Dim sMonth As String = datum.Month.ToString
        Dim sDays As String = Date.DaysInMonth(datum.Year, datum.Month)

        Dim startDate As String = sYear & "/" & sMonth & "/01"
        Dim endDate As String = sYear & "/" & sMonth & "/" & sDays


        ' Dodaj u bazu evidenciju za ovaj dan
        cnData.ConnectionString = ConnectionString
        cnData.Open()

        Dim delSQL As String
        delSQL = "DELETE FROM evd_prisustva_odobrenja WHERE evd_prisustva_id IN(SELECT 	evd_prisustva_id, " & odobravaID & " FROM evd_prisustva WHERE employeeID = " & employeeID & " AND datum >= '" & startDate & "' AND datum <= '" & endDate & "');"
        lcmd = New MySql.Data.MySqlClient.MySqlCommand(delSQL, cnData)
        Try
            lcmd.ExecuteNonQuery()
        Catch ex As Exception

        End Try

        cnData.Close()

    End Sub


    Public Shared Sub PrikaziOdobrenja(ByRef lbl1 As String, ByRef lbl2 As String, ByRef lbl3 As String, ByVal current_employee As Integer, ByVal eYear As Integer, ByVal eMonth As Integer)
        lbl1 = "Nije podnesena!"
        lbl2 = "Nije odobrena!"
        lbl3 = "Nije kontrolisana!"

        Dim cnData As New MySql.Data.MySqlClient.MySqlConnection
        Dim lcmd As MySql.Data.MySqlClient.MySqlCommand
        Dim ldr As MySql.Data.MySqlClient.MySqlDataReader

        ' Dodaj u bazu evidenciju za ovaj dan
        cnData.ConnectionString = ConnectionString
        cnData.Open()

        Dim strSQL As String = "SELECT DISTINCT t1.evd_podnio, e1.LastName, e1.FirstName FROM evd_prisustva t1 " _
                   & "INNER JOIN evd_employees e1 ON t1.evd_podnio = e1.EmployeeID WHERE YEAR(t1.datum) = " & eYear & " AND MONTH(t1.datum) =" & eMonth & " AND t1.employeeID = " & current_employee & ";"

        lcmd = New MySql.Data.MySqlClient.MySqlCommand(strSQL, cnData)
        ldr = lcmd.ExecuteReader
        While ldr.Read
            Try
                lbl1 = ldr.Item("LastName") & " " & ldr.Item("FirstName")
            Catch ex As Exception
                lbl1 = "Greška prilikom oèitavanja baze!"
            End Try

        End While
        ldr.Close()

        strSQL = "SELECT DISTINCT t1.evd_podnio, e1.LastName, e1.FirstName FROM evd_prisustva t1 " _
                   & "INNER JOIN evd_employees e1 ON t1.evd_odobrio = e1.EmployeeID WHERE YEAR(t1.datum) = " & eYear & " AND MONTH(t1.datum) =" & eMonth & " AND t1.employeeID = " & current_employee & ";"

        lcmd.CommandText = strSQL
        ldr = lcmd.ExecuteReader
        While ldr.Read
            Try
                lbl2 = ldr.Item("LastName") & " " & ldr.Item("FirstName")
            Catch ex As Exception
                lbl2 = "Greška prilikom oèitavanja baze!"
            End Try

        End While
        ldr.Close()

        strSQL = "SELECT DISTINCT t1.evd_podnio, e1.LastName, e1.FirstName FROM evd_prisustva t1 " _
                   & "INNER JOIN evd_employees e1 ON t1.evd_kontrolisao = e1.EmployeeID WHERE YEAR(t1.datum) = " & eYear & " AND MONTH(t1.datum) =" & eMonth & " AND t1.employeeID = " & current_employee & ";"

        lcmd.CommandText = strSQL
        ldr = lcmd.ExecuteReader
        While ldr.Read
            Try
                lbl3 = ldr.Item("LastName") & " " & ldr.Item("FirstName")
            Catch ex As Exception
                lbl3 = "Greška prilikom oèitavanja baze!"
            End Try

        End While
        ldr.Close()
        cnData.Close()

    End Sub

    Public Shared Sub Select_Sati(ByRef satiLista As ArrayList, ByVal datum As String, ByVal employeeID As Integer, ByRef odobrenjaLista As ArrayList)


        Dim cnData As New MySql.Data.MySqlClient.MySqlConnection
        Dim lcmd As MySql.Data.MySqlClient.MySqlCommand
        Dim ldr As MySql.Data.MySqlClient.MySqlDataReader
        Dim rptTrajanje As String
        Dim rptDatum As String
        Dim evdPrisustvaID As Integer

        ' Dodaj u bazu evidenciju za ovaj dan
        cnData.ConnectionString = ConnectionString
        cnData.Open()

        Dim strSQL As String = "SELECT t1.evd_prisustva_id, t1.sifra_placanja,  DATE_FORMAT(t1.datum,'%d-%m-%Y') AS datum, DATE_FORMAT(t1.vrijeme_od, '%H:%i') AS vod, DATE_FORMAT(t1.vrijeme_do, '%H:%i') " _
                & " AS vdo, DATE_FORMAT(t1.vrijeme_ukupno, '%H:%i') AS ukupno, t2.naziv " _
                & " FROM evd_prisustva t1 INNER JOIN sfr_placanja t2 on t1.sifra_placanja = t2.sifra WHERE employeeID=" & employeeID.ToString & " AND datum= '" & datum & "';"

        lcmd = New MySql.Data.MySqlClient.MySqlCommand(strSQL, cnData)
        ldr = lcmd.ExecuteReader
        While ldr.Read
            Try
                evdPrisustvaID = ldr.Item("evd_prisustva_id")
                'rptTrajanje = MySQL_ByteToString(ldr.Item("ukupno"))
                'rptDatum = MySQL_ByteToString(ldr.Item("datum"))
                ' TODO: Pretvaranje ByteStream u String datuma upitno!
                rptTrajanje = ldr.Item("ukupno")
                rptDatum = ldr.Item("datum")
            Catch ex As Exception
                rptTrajanje = ""
                rptDatum = ""
            End Try
            satiLista.Add(New EvidencijaDan(ldr.Item("sifra_placanja"), ldr.Item("naziv"), rptDatum, rptTrajanje))
        End While
        ldr.Close()

        strSQL = "SELECT CONCAT(t2.LastName,' ',t2.FirstName) AS EmpName FROM evd_prisustva t1, evd_employees t2 WHERE t1.evd_prisustva_id = " & evdPrisustvaID & " AND t1.employeeID= t2.EmployeeID ORDER BY t1.DummyTime;"
        lcmd.CommandText = strSQL
        ldr = lcmd.ExecuteReader
        While ldr.Read
            Try
                odobrenjaLista.Add(ldr.Item("EmpName"))
            Catch ex As Exception
            End Try

        End While
        ldr.Close()
        cnData.Close()

    End Sub


#Region "**** Odobrenja ****"

    Public Shared Sub OdobrenjeX_Sati(ByVal datum As String, ByVal employeeID As Integer, ByVal selVrstaPlacanja As String, ByVal employeeEvidencija As Integer)
        'Dim cnData As New MySql.Data.MySqlClient.MySqlConnection
        'Dim lcmd As MySql.Data.MySqlClient.MySqlCommand
        Dim cDatabase As ClsDatabase = New ClsDatabase

        If IsOdobrenoBezUposlenikaIRukovodioca_Sati(datum, employeeID) Then
            Exit Sub
        End If

        If selVrstaPlacanja.Length < 3 Then
            Exit Sub
        End If

        ' Dodaj u bazu evidenciju za ovaj dan
        'cnData.ConnectionString = ConnectionString
        'cnData.Open()
        Dim insSQL As String = "INSERT INTO evd_prisustva (employeeID, sifra_placanja, datum, vrijeme_ukupno, evd_podnio, evd_odobrio)	VALUES(" & employeeID.ToString & ", '" & selVrstaPlacanja & "','" & datum & "','08:00:00', " & employeeEvidencija.ToString & "," & employeeEvidencija.ToString & ");"
        Dim delSQL As String = "DELETE FROM evd_prisustva WHERE datum = '" & datum & "' AND employeeID=" & employeeID.ToString & ";"
        'lcmd = New MySql.Data.MySqlClient.MySqlCommand(delSQL, cnData)
        ' lcmd.CommandText = insSQL
        Try
            If selVrstaPlacanja = "DELETE" Then
                'lcmd.ExecuteNonQuery()
                cDatabase.sqlExecute(delSQL)
            Else
                'lcmd.ExecuteNonQuery()
                'lcmd.CommandText = insSQL
                'lcmd.ExecuteNonQuery()
                cDatabase.sqlExecute(delSQL)
                cDatabase.sqlExecute(insSQL)
            End If
        Catch ex As Exception

        End Try

        'cnData.Close()
    End Sub

    Public Shared Sub Odobrenje_Evidencija(ByVal eYear As Integer, ByVal eMonth As Integer, ByVal employeeID As Integer, _
                                           ByVal employeeID_odobrava As Integer, ByVal evd_kontrola As Boolean)
        Dim cnData As New MySql.Data.MySqlClient.MySqlConnection
        Dim lcmd As MySql.Data.MySqlClient.MySqlCommand
        Dim strSQL As String = ""
        ' Dodaj u bazu evidenciju za ovaj dan
        cnData.ConnectionString = ConnectionString
        cnData.Open()

        Select Case evd_kontrola
            Case True
                strSQL = "UPDATE evd_prisustva SET evd_kontrolisao = " & employeeID_odobrava & " WHERE YEAR(datum) = " & eYear & " AND MONTH(datum) =" & eMonth & " AND employeeID=" & employeeID & " AND evd_podnio IS NOT NULL AND evd_odobrio IS NOT NULL;"
            Case False
                If employeeID = employeeID_odobrava Then
                    strSQL = "UPDATE evd_prisustva SET evd_podnio = " & employeeID_odobrava & " WHERE YEAR(datum) = " & eYear & " AND MONTH(datum) =" & eMonth & " AND employeeID=" & employeeID & ";"
                ElseIf employeeID <> employeeID_odobrava Then
                    strSQL = "UPDATE evd_prisustva SET evd_odobrio = " & employeeID_odobrava & " WHERE YEAR(datum) = " & eYear & " AND MONTH(datum) =" & eMonth & " AND employeeID=" & employeeID & " AND evd_podnio IS NOT NULL;"
                End If

        End Select



        lcmd = New MySql.Data.MySqlClient.MySqlCommand(strSQL, cnData)

        Try
            lcmd.ExecuteNonQuery()
        Catch ex As Exception

        End Try


        cnData.Close()

    End Sub

    Public Shared Sub Odobrenje_EvidencijaByRole(ByVal eYear As Integer, ByVal eMonth As Integer, ByVal employeeID As Integer, _
                                           ByVal employeeID_odobrava As Integer, ByVal RoleName As String)
        Dim cnData As New MySql.Data.MySqlClient.MySqlConnection
        Dim lcmd As MySql.Data.MySqlClient.MySqlCommand
        Dim strSQL As String = ""
        ' Dodaj u bazu evidenciju za ovaj dan
        cnData.ConnectionString = ConnectionString
        cnData.Open()

        Select Case RoleName
            Case "pravna"
                strSQL = "UPDATE evd_prisustva SET evd_podnio = " & employeeID_odobrava & " WHERE YEAR(datum) = " & eYear & " AND MONTH(datum) =" & eMonth & " AND employeeID=" & employeeID & " AND evd_locked=0;"
                lcmd = New MySql.Data.MySqlClient.MySqlCommand(strSQL, cnData)
                Try
                    lcmd.ExecuteNonQuery()
                Catch ex As Exception

                End Try
                strSQL = "UPDATE evd_prisustva SET evd_odobrio = " & employeeID_odobrava & " WHERE YEAR(datum) = " & eYear & " AND MONTH(datum) =" & eMonth & " AND employeeID=" & employeeID & " AND evd_podnio IS NOT NULL  AND evd_locked=0;"
                lcmd.CommandText = strSQL
                Try
                    lcmd.ExecuteNonQuery()
                Catch ex As Exception

                End Try

            Case "uposlenik"
                strSQL = "UPDATE evd_prisustva SET evd_podnio = " & employeeID_odobrava & " WHERE YEAR(datum) = " & eYear & " AND MONTH(datum) =" & eMonth & " AND employeeID=" & employeeID & "  AND evd_locked=0;"
                lcmd = New MySql.Data.MySqlClient.MySqlCommand(strSQL, cnData)
                Try
                    lcmd.ExecuteNonQuery()
                Catch ex As Exception
                End Try

            Case "evidencija"
                strSQL = "UPDATE evd_prisustva SET evd_kontrolisao = " & employeeID_odobrava & " WHERE YEAR(datum) = " & eYear & " AND MONTH(datum) =" & eMonth & " AND employeeID=" & employeeID & " AND evd_podnio IS NOT NULL AND evd_odobrio IS NOT NULL  AND evd_locked=0;"
                lcmd = New MySql.Data.MySqlClient.MySqlCommand(strSQL, cnData)
                Try
                    lcmd.ExecuteNonQuery()
                Catch ex As Exception
                End Try

            Case "rukovodilac"
                strSQL = "UPDATE evd_prisustva SET evd_podnio = " & employeeID_odobrava & " WHERE YEAR(datum) = " & eYear & " AND MONTH(datum) =" & eMonth & " AND employeeID=" & employeeID & " AND evd_podnio IS NULL  AND evd_locked=0;"
                lcmd = New MySql.Data.MySqlClient.MySqlCommand(strSQL, cnData)
                Try
                    lcmd.ExecuteNonQuery()
                Catch ex As Exception
                End Try
                strSQL = "UPDATE evd_prisustva SET evd_odobrio = " & employeeID_odobrava & " WHERE YEAR(datum) = " & eYear & " AND MONTH(datum) =" & eMonth & " AND employeeID=" & employeeID & " AND evd_podnio IS NOT NULL  AND evd_locked=0;"
                lcmd = New MySql.Data.MySqlClient.MySqlCommand(strSQL, cnData)
                Try
                    lcmd.ExecuteNonQuery()
                Catch ex As Exception
                End Try

            Case "uprava"
                strSQL = "UPDATE evd_prisustva SET evd_odobrio = " & employeeID_odobrava & " WHERE YEAR(datum) = " & eYear & " AND MONTH(datum) =" & eMonth & " AND employeeID=" & employeeID & " AND evd_podnio IS NOT NULL AND evd_locked=0;"
                lcmd = New MySql.Data.MySqlClient.MySqlCommand(strSQL, cnData)
                Try
                    lcmd.ExecuteNonQuery()
                Catch ex As Exception
                End Try
        End Select




        cnData.Close()

    End Sub

    Public Shared Sub OdobrenjePonisti_Evidencija(ByVal eYear As Integer, ByVal eMonth As Integer, ByVal employeeID As Integer, _
                                           ByVal employeeID_odobrava As Integer, ByVal evd_kontrola As Boolean)
        Dim cnData As New MySql.Data.MySqlClient.MySqlConnection
        Dim lcmd As MySql.Data.MySqlClient.MySqlCommand
        Dim strSQL As String = ""
        ' Dodaj u bazu evidenciju za ovaj dan
        cnData.ConnectionString = ConnectionString
        cnData.Open()

        Select Case evd_kontrola
            Case True
                strSQL = "UPDATE evd_prisustva SET evd_kontrolisao = NULL WHERE YEAR(datum) = " & eYear & " AND MONTH(datum) =" & eMonth & " AND employeeID=" & employeeID & ";"
            Case False
                If employeeID = employeeID_odobrava Then
                    strSQL = "UPDATE evd_prisustva SET evd_podnio = NULL WHERE YEAR(datum) = " & eYear & " AND MONTH(datum) =" & eMonth & " AND employeeID=" & employeeID & " AND evd_odobrio IS NULL;"
                ElseIf employeeID <> employeeID_odobrava Then
                    strSQL = "UPDATE evd_prisustva SET evd_odobrio = NULL WHERE YEAR(datum) = " & eYear & " AND MONTH(datum) =" & eMonth & " AND employeeID=" & employeeID & " AND evd_kontrolisao IS NULL;"
                End If

        End Select



        lcmd = New MySql.Data.MySqlClient.MySqlCommand(strSQL, cnData)

        Try
            lcmd.ExecuteNonQuery()
        Catch ex As Exception

        End Try


        cnData.Close()

    End Sub

    Public Shared Sub OdobrenjePonisti_EvidencijaByRole(ByVal eYear As Integer, ByVal eMonth As Integer, ByVal employeeID As Integer, _
                                       ByVal employeeID_odobrava As Integer, ByVal RoleName As String)
        Dim cnData As New MySql.Data.MySqlClient.MySqlConnection
        Dim lcmd As MySql.Data.MySqlClient.MySqlCommand
        Dim strSQL As String = ""
        ' Dodaj u bazu evidenciju za ovaj dan
        cnData.ConnectionString = ConnectionString
        cnData.Open()


        Select Case RoleName

            Case "pravna"

                strSQL = "UPDATE evd_prisustva SET evd_odobrio = NULL WHERE YEAR(datum) = " & eYear & " AND MONTH(datum) =" & eMonth & " AND employeeID=" & employeeID & " AND evd_kontrolisao IS NULL;"
                lcmd = New MySql.Data.MySqlClient.MySqlCommand(strSQL, cnData)

                Try
                    lcmd.ExecuteNonQuery()
                Catch ex As Exception
                End Try

                strSQL = "UPDATE evd_prisustva SET evd_podnio = NULL WHERE YEAR(datum) = " & eYear & " AND MONTH(datum) =" & eMonth & " AND employeeID=" & employeeID & " AND evd_odobrio IS NULL;"
                lcmd.CommandText = strSQL
                Try
                    lcmd.ExecuteNonQuery()
                Catch ex As Exception
                End Try

            Case "uposlenik"
                strSQL = "UPDATE evd_prisustva SET evd_podnio = NULL WHERE YEAR(datum) = " & eYear & " AND MONTH(datum) =" & eMonth & " AND employeeID=" & employeeID & " AND evd_odobrio IS NULL;"
                lcmd = New MySql.Data.MySqlClient.MySqlCommand(strSQL, cnData)

                Try
                    lcmd.ExecuteNonQuery()
                Catch ex As Exception
                End Try


            Case "evidencija"
                strSQL = "UPDATE evd_prisustva SET evd_kontrolisao = NULL WHERE YEAR(datum) = " & eYear & " AND MONTH(datum) =" & eMonth & " AND employeeID=" & employeeID & ";"
                lcmd = New MySql.Data.MySqlClient.MySqlCommand(strSQL, cnData)

                Try
                    lcmd.ExecuteNonQuery()
                Catch ex As Exception
                End Try

            Case "rukovodilac"
                strSQL = "UPDATE evd_prisustva SET evd_odobrio = NULL WHERE YEAR(datum) = " & eYear & " AND MONTH(datum) =" & eMonth & " AND employeeID=" & employeeID & " AND evd_kontrolisao IS NULL;"
                lcmd = New MySql.Data.MySqlClient.MySqlCommand(strSQL, cnData)

                Try
                    lcmd.ExecuteNonQuery()
                Catch ex As Exception
                End Try

            Case "uprava"
                strSQL = "UPDATE evd_prisustva SET evd_odobrio = NULL WHERE YEAR(datum) = " & eYear & " AND MONTH(datum) =" & eMonth & " AND employeeID=" & employeeID & " AND evd_kontrolisao IS NULL;"
                lcmd = New MySql.Data.MySqlClient.MySqlCommand(strSQL, cnData)

                Try
                    lcmd.ExecuteNonQuery()
                Catch ex As Exception
                End Try
        End Select


        cnData.Close()

    End Sub

    Public Shared Function OdodbrenjeIsKontrola_Evidencija(ByVal employeeID As Integer) As Boolean
        Dim cnData As New MySql.Data.MySqlClient.MySqlConnection
        Dim lcmd As MySql.Data.MySqlClient.MySqlCommand
        Dim ldr As MySql.Data.MySqlClient.MySqlDataReader
        Dim strSQL As String = ""
        ' Dodaj u bazu evidenciju za ovaj dan
        cnData.ConnectionString = ConnectionString
        cnData.Open()


        strSQL = "SELECT roleId FROM my_aspnet_usersinroles t1 INNER JOIN my_aspnet_users_to_employees t2 ON t1.userId = t2.users_id" _
                                & " WHERE t1.roleId = 6 AND t2.employees_id = " & employeeID & " ;"

        lcmd = New MySql.Data.MySqlClient.MySqlCommand(strSQL, cnData)
        ldr = lcmd.ExecuteReader
        If ldr.HasRows Then
            Return True
        Else
            Return False
        End If

        ldr.Close()
        cnData.Close()

    End Function

#End Region




    Public Shared Sub PopuniZbirnuListu_OneEmployee(ByVal eYear As Integer, ByVal eMonth As Integer, ByVal EmployeeId As Integer)

        Dim strSql As String = "UPDATE `xfc_uposleni-placanja` " _
            & "SET Iznos_Sati = 0, Iznos_KonObr = 0 WHERE	EmployeeID = " & EmployeeId & ";"

        Dim cnData As New MySql.Data.MySqlClient.MySqlConnection
        Dim lcmd As MySql.Data.MySqlClient.MySqlCommand

        cnData.ConnectionString = ConnectionString
        cnData.Open()

        lcmd = New MySql.Data.MySqlClient.MySqlCommand(strSql, cnData)
        Try
            lcmd.ExecuteNonQuery()
        Catch ex As Exception
        End Try

        strSql = "UPDATE `xfc_uposleni-placanja` AS t1, (SELECT HOUR((SEC_TO_TIME( SUM( TIME_TO_SEC( vrijeme_ukupno ))))) AS ukupno  FROM evd_prisustva " _
            & " WHERE  	employeeID = " & EmployeeId & " AND sifra_placanja='P03' AND YEAR(datum)=" & eYear & " AND MONTH(datum)=" & eMonth & ") AS t2 " _
            & "SET t1.Iznos_Sati = t2.ukupno WHERE	t1.EmployeeID = " & EmployeeId & "  AND t1.Sifra = 'P03' ;"
        lcmd.CommandText = strSql
        Try
            lcmd.ExecuteNonQuery()
        Catch ex As Exception
        End Try

        strSql = "UPDATE `xfc_uposleni-placanja` AS t1, (SELECT HOUR((SEC_TO_TIME( SUM( TIME_TO_SEC( vrijeme_ukupno ))))) AS ukupno  FROM evd_prisustva " _
            & " WHERE  	employeeID = " & EmployeeId & " AND sifra_placanja='P04' AND YEAR(datum)=" & eYear & " AND MONTH(datum)=" & eMonth & ") AS t2 " _
            & "SET t1.Iznos_Sati = t2.ukupno WHERE	t1.EmployeeID = " & EmployeeId & "  AND t1.Sifra = 'P04' ;"
        lcmd.CommandText = strSql
        Try
            lcmd.ExecuteNonQuery()
        Catch ex As Exception
        End Try

        strSql = "UPDATE `xfc_uposleni-placanja` AS t1, (SELECT HOUR((SEC_TO_TIME( SUM( TIME_TO_SEC( vrijeme_ukupno ))))) AS ukupno  FROM evd_prisustva " _
            & " WHERE  	employeeID = " & EmployeeId & " AND sifra_placanja='P05' AND YEAR(datum)=" & eYear & " AND MONTH(datum)=" & eMonth & ") AS t2 " _
            & "SET t1.Iznos_Sati = t2.ukupno WHERE	t1.EmployeeID = " & EmployeeId & "  AND t1.Sifra = 'P05' ;"
        lcmd.CommandText = strSql
        Try
            lcmd.ExecuteNonQuery()
        Catch ex As Exception
        End Try


        strSql = "UPDATE `xfc_uposleni-placanja` AS t1, (SELECT HOUR((SEC_TO_TIME( SUM( TIME_TO_SEC( vrijeme_ukupno ))))) AS ukupno  FROM evd_prisustva " _
            & " WHERE  	employeeID = " & EmployeeId & " AND sifra_placanja='P06' AND YEAR(datum)=" & eYear & " AND MONTH(datum)=" & eMonth & ") AS t2 " _
            & "SET t1.Iznos_Sati = t2.ukupno WHERE	t1.EmployeeID = " & EmployeeId & "  AND t1.Sifra = 'P06' ;"
        lcmd.CommandText = strSql
        Try
            lcmd.ExecuteNonQuery()
        Catch ex As Exception
        End Try


        strSql = "UPDATE `xfc_uposleni-placanja` AS t1, (SELECT HOUR((SEC_TO_TIME( SUM( TIME_TO_SEC( vrijeme_ukupno ))))) AS ukupno  FROM evd_prisustva " _
            & " WHERE  	employeeID = " & EmployeeId & " AND sifra_placanja='P09' AND YEAR(datum)=" & eYear & " AND MONTH(datum)=" & eMonth & ") AS t2 " _
            & "SET t1.Iznos_Sati = t2.ukupno WHERE	t1.EmployeeID = " & EmployeeId & "  AND t1.Sifra = 'P09' ;"
        lcmd.CommandText = strSql
        Try
            lcmd.ExecuteNonQuery()
        Catch ex As Exception
        End Try


        strSql = "UPDATE `xfc_uposleni-placanja` AS t1, (SELECT HOUR((SEC_TO_TIME( SUM( TIME_TO_SEC( vrijeme_ukupno ))))) AS ukupno  FROM evd_prisustva " _
            & " WHERE  	employeeID = " & EmployeeId & " AND sifra_placanja='P20' AND YEAR(datum)=" & eYear & " AND MONTH(datum)=" & eMonth & ") AS t2 " _
            & "SET t1.Iznos_Sati = t2.ukupno WHERE	t1.EmployeeID = " & EmployeeId & "  AND t1.Sifra = 'P20' ;"
        lcmd.CommandText = strSql
        Try
            lcmd.ExecuteNonQuery()
        Catch ex As Exception
        End Try

        strSql = "UPDATE `xfc_uposleni-placanja` AS t1, (SELECT HOUR((SEC_TO_TIME( SUM( TIME_TO_SEC( vrijeme_ukupno ))))) AS ukupno  FROM evd_prisustva " _
            & " WHERE  	employeeID = " & EmployeeId & " AND sifra_placanja='P21' AND YEAR(datum)=" & eYear & " AND MONTH(datum)=" & eMonth & ") AS t2 " _
            & "SET t1.Iznos_Sati = t2.ukupno WHERE	t1.EmployeeID = " & EmployeeId & "  AND t1.Sifra = 'P21' ;"
        lcmd.CommandText = strSql
        Try
            lcmd.ExecuteNonQuery()
        Catch ex As Exception
        End Try


        strSql = "UPDATE `xfc_uposleni-placanja` AS t1, (SELECT HOUR((SEC_TO_TIME( SUM( TIME_TO_SEC( vrijeme_ukupno ))))) AS ukupno  FROM evd_prisustva " _
            & " WHERE  	employeeID = " & EmployeeId & " AND sifra_placanja='P51' AND YEAR(datum)=" & eYear & " AND MONTH(datum)=" & eMonth & ") AS t2 " _
            & "SET t1.Iznos_Sati = t2.ukupno WHERE	t1.EmployeeID = " & EmployeeId & "  AND t1.Sifra = 'P51' ;"
        lcmd.CommandText = strSql
        Try
            lcmd.ExecuteNonQuery()
        Catch ex As Exception
        End Try

        strSql = "UPDATE `xfc_uposleni-placanja` AS t1, (SELECT HOUR((SEC_TO_TIME( SUM( TIME_TO_SEC( vrijeme_ukupno ))))) AS ukupno  FROM evd_prisustva " _
            & " WHERE  	employeeID = " & EmployeeId & " AND sifra_placanja='P52' AND YEAR(datum)=" & eYear & " AND MONTH(datum)=" & eMonth & ") AS t2 " _
            & "SET t1.Iznos_Sati = t2.ukupno WHERE	t1.EmployeeID = " & EmployeeId & "  AND t1.Sifra = 'P52' ;"
        lcmd.CommandText = strSql
        Try
            lcmd.ExecuteNonQuery()
        Catch ex As Exception
        End Try

        strSql = "UPDATE `xfc_uposleni-placanja` AS t1, (SELECT HOUR((SEC_TO_TIME( SUM( TIME_TO_SEC( vrijeme_ukupno ))))) AS ukupno  FROM evd_prisustva " _
            & " WHERE  	employeeID = " & EmployeeId & " AND sifra_placanja='P53' AND YEAR(datum)=" & eYear & " AND MONTH(datum)=" & eMonth & ") AS t2 " _
            & "SET t1.Iznos_Sati = t2.ukupno WHERE	t1.EmployeeID = " & EmployeeId & "  AND t1.Sifra = 'P53' ;"
        lcmd.CommandText = strSql
        Try
            lcmd.ExecuteNonQuery()
        Catch ex As Exception
        End Try

        strSql = "UPDATE `xfc_uposleni-placanja` AS t1, (SELECT HOUR((SEC_TO_TIME( SUM( TIME_TO_SEC( vrijeme_ukupno ))))) AS ukupno  FROM evd_prisustva " _
            & " WHERE  	employeeID = " & EmployeeId & " AND sifra_placanja='P56' AND YEAR(datum)=" & eYear & " AND MONTH(datum)=" & eMonth & ") AS t2 " _
            & "SET t1.Iznos_Sati = t2.ukupno WHERE	t1.EmployeeID = " & EmployeeId & "  AND t1.Sifra = 'P56' ;"
        lcmd.CommandText = strSql
        Try
            lcmd.ExecuteNonQuery()
        Catch ex As Exception
        End Try

        cnData.Close()

    End Sub

    Public Shared Sub PopuniZbirnuListu_AllEmployees(ByVal eYear As Integer, ByVal eMonth As Integer)
        Dim cnData As New MySql.Data.MySqlClient.MySqlConnection
        Dim lcmd As MySql.Data.MySqlClient.MySqlCommand
        Dim ldr As MySql.Data.MySqlClient.MySqlDataReader
        Dim strSQL As String = ""
        Dim listEmployees As New ArrayList


        ' Dodaj u bazu evidenciju za ovaj dan
        cnData.ConnectionString = ConnectionString
        cnData.Open()


        strSQL = "SELECT DISTINCT EmployeeID FROM `xfc_uposleni-placanja`;"

        lcmd = New MySql.Data.MySqlClient.MySqlCommand(strSQL, cnData)
        ldr = lcmd.ExecuteReader
        If ldr.HasRows Then
            While ldr.Read
                Try
                    listEmployees.Add(ldr.Item("EmployeeId"))
                Catch ex As Exception

                End Try

            End While
        Else

        End If

        ldr.Close()
        cnData.Close()

        For Each oneEmployee As Integer In listEmployees
            PopuniZbirnuListu_OneEmployee(eYear, eMonth, oneEmployee)
        Next


    End Sub

    Public Shared Function FetchEvidancijeSati(Optional ByVal EmployeeID As Integer = 0) As System.Data.DataTable
        Dim cnData As New MySql.Data.MySqlClient.MySqlConnection
        Dim strSQL As String = "SELECT * FROM `xfc_uposleni-placanja` WHERE EmployeeID=" & EmployeeID & " AND (Sifra LIKE 'P0_' OR Sifra LIKE 'P5_' OR Sifra LIKE 'P2_') ORDER BY Sifra;"

        cnData.ConnectionString = ConnectionString
        cnData.Open()
        Dim da As New MySqlClient.MySqlDataAdapter(strSQL, cnData)
        Dim dt As New Data.DataTable()
        da.Fill(dt)
        Return dt
    End Function

    Public Shared Sub getEvidencijeLista_Odobravanje(ByRef Repeater1 As Repeater, ByVal sYear As Integer, ByVal sMonth As Integer, _
                        ByVal EmployeeID As Integer, ByVal curRole As String)
        Dim con As New MySqlClient.MySqlConnection(ConnectionString)

        Dim EmployeesList As String = ClsPutniNalozi.Uposlenici_ListaByRole(curRole, EmployeeID)

        Dim sqlText As String = "(SELECT DISTINCT e1.employeeID, CONCAT(e1.LastName,' ',e1.FirstName) AS Employee, IF(e2.LastName IS NULL,'...',CONCAT(e2.LastName,' ',e2.FirstName)) AS podnio " _
                            & ",IF(e3.LastName IS NULL,'...',CONCAT(e3.LastName,' ',e3.FirstName)) AS odobrio, IF(e4.LastName IS NULL,'...', CONCAT(e4.LastName,' ',e4.FirstName)) AS kontrolisao " _
                            & " FROM evd_employees AS e1 LEFT JOIN evd_prisustva AS ep " _
                            & " ON (ep.employeeID = e1.EmployeeID) LEFT JOIN evd_employees AS e2 ON (ep.evd_podnio = e2.EmployeeID) LEFT JOIN evd_employees AS e3 " _
                            & " ON (ep.evd_odobrio = e3.EmployeeID) LEFT JOIN evd_employees AS e4 ON (ep.evd_kontrolisao = e4.EmployeeID) " _
                            & " WHERE ((YEAR(ep.datum)=" & sYear & " AND MONTH(ep.datum)=" & sMonth & ") AND e1.employeeID IN (" & EmployeesList & "))) " _
                            & " UNION" _
                            & "((SELECT DISTINCT e1.employeeID, CONCAT(e1.LastName,' ',e1.FirstName) AS Employee, '...' AS podnio ,'...' AS odobrio, '...' AS kontrolisao  " _
                            & "FROM evd_employees AS e1 LEFT JOIN evd_prisustva AS ep  ON (ep.employeeID = e1.EmployeeID) " _
                            & "LEFT JOIN evd_employees AS e2 ON (ep.evd_podnio = e2.EmployeeID) " _
                            & "LEFT JOIN evd_employees AS e3  ON (ep.evd_odobrio = e3.EmployeeID) " _
                            & "LEFT JOIN evd_employees AS e4 ON (ep.evd_kontrolisao = e4.EmployeeID) " _
                            & "WHERE (e1.employeeID IN (" & EmployeesList & ") " _
                            & "AND e1.employeeID NOT IN (SELECT DISTINCT employeeID FROM evd_prisustva ep WHERE (YEAR(ep.datum)=" & sYear & " AND MONTH(ep.datum)=" & sMonth & ")))" _
                            & " ORDER BY Employee))" _
                            & "  ORDER BY Employee;"

        Dim cmd As New MySqlClient.MySqlCommand(sqlText, con)

        con.Open()
        Try
            Dim rd As MySqlClient.MySqlDataReader = cmd.ExecuteReader()
            Repeater1.DataSource = rd
            Repeater1.DataBind()
        Catch ex As Exception

        End Try

        con.Close()
    End Sub

    Public Shared Sub getEvidencijeLista_Bolovanje(ByRef Repeater1 As Repeater, ByVal sYear As Integer, ByVal sMonth As Integer, _
                     ByVal EmployeeID As Integer, ByVal curRole As String)
        Dim con As New MySqlClient.MySqlConnection(ConnectionString)

        Dim EmployeesList As String = ClsPutniNalozi.Uposlenici_ListaByRole(curRole, EmployeeID)

        Dim sqlText As String = "SELECT DISTINCT e1.employeeID, CONCAT(e1.LastName,' ',e1.FirstName) AS Employee, '...' AS podnio, '...' AS odobrio, '...' AS kontrolisao  FROM evd_employees AS e1 " _
                              & " WHERE  e1.employeeID IN (" & EmployeesList & ") ORDER BY Employee;"

        Dim cmd As New MySqlClient.MySqlCommand(sqlText, con)

        con.Open()
        Try
            Dim rd As MySqlClient.MySqlDataReader = cmd.ExecuteReader()
            Repeater1.DataSource = rd
            Repeater1.DataBind()
        Catch ex As Exception

        End Try

        con.Close()
    End Sub

    Public Shared Sub zatvoriEvidencije(ByVal startDatum As DateTime, ByVal endDatum As DateTime)
        Dim db As ClsDatabase = New ClsDatabase
        Dim sqlText As String
        Dim startDat As String = ClsDatabase.Date_MySQLFormat(startDatum)
        Dim endDat As String = ClsDatabase.Date_MySQLFormat(endDatum)

        Try
            sqlText = "DELETE FROM evd_prisustva_suma WHERE mjesec=" & Month(startDatum) & " AND godina = " & Year(startDatum) & ";"
            db.sqlExecute(sqlText)

            sqlText = "INSERT INTO evd_prisustva_suma(employeeID,sifra_placanja,suma_sati,mjesec,godina)" _
                    & " SELECT employeeID, sifra_placanja, SUM(vrijeme_ukupno)/10000 AS suma_sati, " & Month(startDatum) & ", " & Year(startDatum) & " " _
                    & " FROM evd_prisustva WHERE DATE(datum)>=DATE('" & startDat & "') AND DATE(datum)<=DATE('" & endDat & "') " _
                    & " GROUP BY  employeeID, sifra_placanja ORDER BY employeeID;"
            db.sqlExecute(sqlText)

            sqlText = "UPDATE evd_prisustva t1 SET t1.evd_locked = -1 WHERE DATE(datum)>=DATE('" & startDat & "') AND DATE(datum)<=DATE('" & endDat & "');"
            db.sqlExecute(sqlText)

            sqlText = "DELETE FROM evd_godisnjiodmor_update WHERE mjesec=" & Month(startDatum) & " AND godina = " & Year(startDatum) & ";"
            db.sqlExecute(sqlText)

            sqlText = "INSERT INTO evd_godisnjiodmor_update (employeeID, UsedDays, Godina,mjesec)" _
                    & " SELECT employeeID, suma_sati/8, godina, mjesec FROM evd_prisustva_suma " _
                    & " WHERE godina = " & Year(startDatum) & " AND mjesec = " & Month(startDatum) & " AND sifra_placanja='P20';"
            db.sqlExecute(sqlText)

            sqlText = "UPDATE evd_godisnjiodmor_update t1, (SELECT t2.employeeID, IFNULL(t1.IzrDani-t1.IskDani,0) AS RaspDaniPrev, t2.IzrDani-t2.IskDani AS RaspDaniCur" _
                    & " FROM evd_godisnjiodmor t1 RIGHT JOIN evd_godisnjiodmor t2 ON (t1.employeeID = t2.employeeID AND t1.Godina+1 = t2.Godina)" _
                    & " WHERE  ( t2.Godina = " & Year(startDatum) - 1 & ") ORDER BY t2.employeeID) AS t2 " _
                    & " SET t1.PrevYearUpdate = t2.RaspDaniPrev, t1.CurYearUpdate = t2.RaspDaniCur " _
                    & " WHERE t1.employeeID = t2.employeeID AND t1.Godina = " & Year(startDatum) & " AND t1.mjesec = " & Month(startDatum) & ";"
            db.sqlExecute(sqlText)

            sqlText = "UPDATE evd_godisnjiodmor_update t1 SET PrevYearUpdate = UsedDays WHERE PrevYearUpdate - UsedDays >0 AND " _
                    & " t1.Godina=" & Year(startDatum) & " AND t1.mjesec = " & Month(startDatum) & ";"
            db.sqlExecute(sqlText)

            sqlText = "UPDATE evd_godisnjiodmor_update t1 SET CurYearUpdate = UsedDays-PrevYearUpdate WHERE " _
                    & " t1.Godina=" & Year(startDatum) & " AND t1.mjesec = " & Month(startDatum) & ";"
            db.sqlExecute(sqlText)

            sqlText = "UPDATE evd_godisnjiodmor t1, evd_godisnjiodmor_update t2 SET t1.IskDani = t1.IskDani + t2.CurYearUpdate " _
                    & " WHERE t1.employeeID = t2.employeeID AND t1.Godina = t2.Godina AND t2.Godina =" & Year(startDatum) & " AND t2.mjesec = " & Month(startDatum) & ";"
            db.sqlExecute(sqlText)

            sqlText = "UPDATE evd_godisnjiodmor t1, evd_godisnjiodmor_update t2 SET t1.IskDani = t1.IskDani + t2.PrevYearUpdate " _
                    & " WHERE t1.employeeID = t2.employeeID AND t1.Godina = t2.Godina-1 AND t2.Godina =" & Year(startDatum) & " AND t2.mjesec = " & Month(startDatum) & ";"
            db.sqlExecute(sqlText)


            sqlText = "DELETE FROM `XFC_Uposleni-Placanja`;"
            db.sqlExecute(sqlText)

            sqlText = " INSERT INTO `XFC_Uposleni-Placanja`(Placanjaid, EmployeeID, Sifra, Naziv, Vrsta,  Atribut_1, Atribut_2, Satnica, TopliObrok, Koeficijent, Iznos_Sati, Iznos_KonObr, " _
                    & " Iznos_FinObr,Iznos_BrutoObr) SELECT T2.id, T1.EmployeeID, T2.Sifra, T2.Naziv, T2.Vrsta, T2.Atribut_1, T2.Atribut_2, T2.Satnica, T2.TopliObrok, T2.Koeficijent, T2.Iznos_Sati," _
                    & " T2.Iznos_KonObr, T2.Iznos_FinObr, T2.Iznos_BrutoObr " _
                    & " FROM evd_employees AS T1 INNER JOIN sfr_organizacija AS T1A ON T1.DepartmentUP = T1A.id " _
                    & " , SFR_Placanja AS T2 WHERE T1.Aktivan <> 0 AND (T1A.Atribut_1 <> 'VIRTUAL' OR T1A.Atribut_1 IS NULL);"
            db.sqlExecute(sqlText)

            sqlText = "UPDATE `xfc_uposleni-placanja` t1, evd_prisustva_suma t2 SET t1.Iznos_Sati = t2.suma_sati " _
                    & " WHERE t1.Sifra = t2.sifra_placanja AND t1.EmployeeID = t2.employeeID AND t2.mjesec = " & Month(startDatum) & " AND t2.godina = " & Year(startDatum) & ";"
            db.sqlExecute(sqlText)
        Catch ex As Exception

        End Try



    End Sub

    Public Shared Sub ponistiZatvoriEvidencije(ByVal startDatum As DateTime, ByVal endDatum As DateTime)
        Dim db As ClsDatabase = New ClsDatabase
        Dim sqlText As String
        Dim startDat As String = ClsDatabase.Date_MySQLFormat(startDatum)
        Dim endDat As String = ClsDatabase.Date_MySQLFormat(endDatum)

        Try
            sqlText = "UPDATE evd_prisustva t1 SET t1.evd_locked = 0 WHERE DATE(datum)>=DATE('" & startDat & "') AND DATE(datum)<=DATE('" & endDat & "');"
            db.sqlExecute(sqlText)


            sqlText = "UPDATE evd_godisnjiodmor t1, evd_godisnjiodmor_update t2 SET t1.IskDani = t1.IskDani - t2.CurYearUpdate " _
                    & " WHERE t1.employeeID = t2.employeeID AND t1.Godina = t2.Godina AND t2.Godina =" & Year(startDatum) & " AND t2.mjesec = " & Month(startDatum) & ";"
            db.sqlExecute(sqlText)

            sqlText = "UPDATE evd_godisnjiodmor t1, evd_godisnjiodmor_update t2 SET t1.IskDani = t1.IskDani - t2.PrevYearUpdate " _
                    & " WHERE t1.employeeID = t2.employeeID AND t1.Godina = t2.Godina-1 AND t2.Godina =" & Year(startDatum) & " AND t2.mjesec = " & Month(startDatum) & ";"
            db.sqlExecute(sqlText)

            sqlText = "DELETE FROM evd_prisustva_suma WHERE mjesec=" & Month(startDatum) & " AND godina = " & Year(startDatum) & ";"
            db.sqlExecute(sqlText)

            sqlText = "DELETE FROM evd_godisnjiodmor_update WHERE mjesec=" & Month(startDatum) & " AND godina = " & Year(startDatum) & ";"
            db.sqlExecute(sqlText)

            sqlText = "DELETE FROM `XFC_Uposleni-Placanja`;"
            db.sqlExecute(sqlText)

        Catch ex As Exception

        End Try



    End Sub

    Public Shared Sub kumulativneEvidencije(ByVal startDatum As DateTime, ByVal endDatum As DateTime)
        '
        'USLOV: Evidencija.IsEvidencija_Zakljucana(curMonth, curYear) = False

        Dim db As ClsDatabase = New ClsDatabase
        Dim sqlText As String
        Dim startDat As String = ClsDatabase.Date_MySQLFormat(startDatum)
        Dim endDat As String = ClsDatabase.Date_MySQLFormat(endDatum)


        Try

            sqlText = "DELETE FROM evd_prisustva_suma WHERE mjesec=" & Month(startDatum) & " AND godina = " & Year(startDatum) & ";"
            db.sqlExecute(sqlText)

            sqlText = "INSERT INTO evd_prisustva_suma(employeeID,sifra_placanja,suma_sati,mjesec,godina)" _
                    & " SELECT employeeID, sifra_placanja, SUM(vrijeme_ukupno)/10000 AS suma_sati, " & Month(startDatum) & ", " & Year(startDatum) & " " _
                    & " FROM evd_prisustva WHERE DATE(datum)>=DATE('" & startDat & "') AND DATE(datum)<=DATE('" & endDat & "') " _
                    & " GROUP BY  employeeID, sifra_placanja ORDER BY employeeID;"
            db.sqlExecute(sqlText)

            sqlText = "DELETE FROM `XFC_Uposleni-Placanja`;"
            db.sqlExecute(sqlText)

            sqlText = " INSERT INTO `XFC_Uposleni-Placanja`(Placanjaid, EmployeeID, Sifra, Naziv, Vrsta,  Atribut_1, Atribut_2, Satnica, TopliObrok, Koeficijent, Iznos_Sati, Iznos_KonObr, " _
                    & " Iznos_FinObr,Iznos_BrutoObr) SELECT T2.id, T1.EmployeeID, T2.Sifra, T2.Naziv, T2.Vrsta, T2.Atribut_1, T2.Atribut_2, T2.Satnica, T2.TopliObrok, T2.Koeficijent, T2.Iznos_Sati," _
                    & " T2.Iznos_KonObr, T2.Iznos_FinObr, T2.Iznos_BrutoObr  FROM evd_employees AS T1, SFR_Placanja AS T2 WHERE T1.Aktivan <> 0;"
            db.sqlExecute(sqlText)

            sqlText = "UPDATE `xfc_uposleni-placanja` t1, evd_prisustva_suma t2 SET t1.Iznos_Sati = t2.suma_sati " _
                    & " WHERE t1.Sifra = t2.sifra_placanja AND t1.EmployeeID = t2.employeeID AND t2.mjesec = " & Month(startDatum) & " AND t2.godina = " & Year(startDatum) & ";"
            db.sqlExecute(sqlText)


        Catch ex As Exception

        End Try



    End Sub

    Public Shared Sub kumulativneEvidencijeXFC(ByVal startDatum As DateTime, ByVal endDatum As DateTime)
        ' 
        ' Uslov: Evidencija.IsEvidencija_Zakljucana(curMonth, curYear) = True

        Dim db As ClsDatabase = New ClsDatabase
        Dim sqlText As String
        Dim startDat As String = ClsDatabase.Date_MySQLFormat(startDatum)
        Dim endDat As String = ClsDatabase.Date_MySQLFormat(endDatum)

        Try
            sqlText = "DELETE FROM `XFC_Uposleni-Placanja`;"
            db.sqlExecute(sqlText)

            sqlText = " INSERT INTO `XFC_Uposleni-Placanja`(Placanjaid, EmployeeID, Sifra, Naziv, Vrsta,  Atribut_1, Atribut_2, Satnica, TopliObrok, Koeficijent, Iznos_Sati, Iznos_KonObr, " _
                    & " Iznos_FinObr,Iznos_BrutoObr) SELECT T2.id, T1.EmployeeID, T2.Sifra, T2.Naziv, T2.Vrsta, T2.Atribut_1, T2.Atribut_2, T2.Satnica, T2.TopliObrok, T2.Koeficijent, T2.Iznos_Sati," _
                    & " T2.Iznos_KonObr, T2.Iznos_FinObr, T2.Iznos_BrutoObr " _
                    & " FROM evd_employees AS T1 INNER JOIN sfr_organizacija AS T1A ON T1.DepartmentUP = T1A.id " _
                    & " , SFR_Placanja AS T2 WHERE T1.Aktivan <> 0 AND (T1A.Atribut_1 <> 'VIRTUAL' OR T1A.Atribut_1 IS NULL);"
            db.sqlExecute(sqlText)

            sqlText = "UPDATE `xfc_uposleni-placanja` t1, evd_prisustva_suma t2 SET t1.Iznos_Sati = t2.suma_sati " _
                    & " WHERE t1.Sifra = t2.sifra_placanja AND t1.EmployeeID = t2.employeeID AND t2.mjesec = " & Month(startDatum) & " AND t2.godina = " & Year(startDatum) & ";"
            db.sqlExecute(sqlText)

        Catch ex As Exception

        End Try



    End Sub

    Public Shared Sub kumulativneEvidencijeVirtualXFC(ByVal startDatum As DateTime, ByVal endDatum As DateTime, ByRef portalTableSet As DataSet)
        '--------------------------------------------------------------------------------------
        ' Ovu proceduru ne prozivati direktno nego preko procedure ClsXmlZip.ExportDatasetToCsv
        '

        Dim db As ClsDatabase = New ClsDatabase
        Dim sqlText As String
        Dim startDat As String = ClsDatabase.Date_MySQLFormat(startDatum)
        Dim endDat As String = ClsDatabase.Date_MySQLFormat(endDatum)

        db.connOpen()
        db.connCreateCommand()

        sqlText = "DROP TABLE IF EXISTS `XFC_Uposleni-PlacanjaVirtual`;"
        db.sqlExecute_LeaveOpenConn(sqlText)

        sqlText = " CREATE TEMPORARY TABLE `XFC_Uposleni-PlacanjaVirtual` " _
                & " SELECT T2.id, T1.EmployeeID, T2.Sifra, T2.Naziv, T2.Vrsta, T2.Atribut_1, T2.Atribut_2, T2.Satnica, T2.TopliObrok, T2.Koeficijent, T2.Iznos_Sati," _
                & " T2.Iznos_KonObr, T2.Iznos_FinObr, T2.Iznos_BrutoObr " _
                & " FROM evd_employees AS T1 INNER JOIN sfr_organizacija AS T1A ON T1.DepartmentUP = T1A.id " _
                & " , SFR_Placanja AS T2 WHERE T1.Aktivan <> 0 AND (T1A.Atribut_1 = 'VIRTUAL');"
        db.sqlExecute_LeaveOpenConn(sqlText)

        sqlText = "UPDATE `XFC_Uposleni-PlacanjaVirtual` t1, evd_prisustva_suma t2 SET t1.Iznos_Sati = t2.suma_sati " _
                & " WHERE t1.Sifra = t2.sifra_placanja AND t1.EmployeeID = t2.employeeID AND t2.mjesec = " & Month(startDatum) & " AND t2.godina = " & Year(startDatum) & ";"
        db.sqlExecute_LeaveOpenConn(sqlText)

        Dim adap As MySqlClient.MySqlDataAdapter
        Dim cmd As MySqlClient.MySqlCommand
        Dim sqlSelect As String = "SELECT * FROM `XFC_Uposleni-PlacanjaVirtual`;"

        cmd = db.Connection.CreateCommand()
        cmd.CommandText = sqlSelect

        adap = New MySqlClient.MySqlDataAdapter()
        adap.SelectCommand = cmd
        adap.Fill(portalTableSet, "XFC_Uposleni-PlacanjaVirtual")

        db.connClose()

    End Sub

    Public Shared Sub PrikaziDatum_KumulativneEvidencije(ByRef lbl1 As String)
        lbl1 = "Nema kumulativnih evidencija!"

        Dim cnData As New MySql.Data.MySqlClient.MySqlConnection
        Dim lcmd As MySql.Data.MySqlClient.MySqlCommand
        Dim ldr As MySql.Data.MySqlClient.MySqlDataReader

        ' Dodaj u bazu evidenciju za ovaj dan
        cnData.ConnectionString = ConnectionString
        cnData.Open()

        Dim strSQL As String = "SELECT  mjesec, godina FROM evd_prisustva_suma ORDER BY godina, mjesec DESC LIMIT 1;"

        lcmd = New MySql.Data.MySqlClient.MySqlCommand(strSQL, cnData)
        ldr = lcmd.ExecuteReader
        While ldr.Read
            Try
                lbl1 = "Kumulativna evidnecija za " & ldr.Item("mjesec") & "-" & ldr.Item("godina")
            Catch ex As Exception
                lbl1 = "Nema kumulativnih evidencija!"
            End Try

        End While
        ldr.Close()
        cnData.Close()

    End Sub

    Public Shared Function IsEvidencija_Zakljucana(ByVal curMonth As Integer, ByVal curYear As Integer) As Boolean

        Dim cnData As New MySql.Data.MySqlClient.MySqlConnection
        Dim lcmd As MySql.Data.MySqlClient.MySqlCommand
        Dim ldr As MySql.Data.MySqlClient.MySqlDataReader

        Dim retValue As Boolean = False
        ' Dodaj u bazu evidenciju za ovaj dan
        cnData.ConnectionString = ConnectionString
        cnData.Open()

        Dim strSQL As String = "SELECT COUNT(employeeID) AS Locked FROM evd_godisnjiodmor_update WHERE godina=" & curYear & " AND mjesec = " & curMonth & ";"
        strSQL = "SELECT COUNT(employeeID) AS Locked FROM `evd_prisustva` WHERE YEAR(datum)=" & curYear & " AND MONTH(datum) = " & curMonth & " AND evd_locked<>0;"

        lcmd = New MySql.Data.MySqlClient.MySqlCommand(strSQL, cnData)
        ldr = lcmd.ExecuteReader
        While ldr.Read
            Try
                If ldr.Item("Locked") > 0 Then
                    retValue = True
                End If
            Catch ex As Exception
            End Try

        End While
        ldr.Close()
        cnData.Close()

        Return retValue

    End Function


#Region "**** Zahtjevi ****"

    Public Shared Sub Zahtjevi_Lista(ByVal EmployeeID As Integer, ByRef zahtjeviLista As ArrayList, _
                           ByRef clsMySqlConnection As MySql.Data.MySqlClient.MySqlConnection)

        Dim lcmd As MySql.Data.MySqlClient.MySqlCommand
        Dim ldr As MySql.Data.MySqlClient.MySqlDataReader
        Dim strSQL As String = ""

        strSQL = "SELECT zahtjevi_id, datum FROM evd_zahtjevi t1 WHERE employeeID = " & EmployeeID & " AND zahtjevi_id  IN " _
        & "(SELECT DISTINCT zahtjevi_id FROM evd_prisustva_zahtjevi WHERE employeeID = " & EmployeeID & " AND (evd_odobrio IS NOT NULL OR evd_kontrolisao IS NOT NULL) ORDER BY zahtjevi_id );"
        lcmd = New MySql.Data.MySqlClient.MySqlCommand(strSQL, clsMySqlConnection)

        Dim clsDatum As MySql.Data.Types.MySqlDateTime

        ldr = lcmd.ExecuteReader
        While ldr.Read
            Try
                clsDatum = ldr.Item("datum")
                zahtjeviLista.Add(New EvidencijaZahtjevi(ldr.Item("zahtjevi_id"), EmployeeID, "datum : " & clsDatum.Day & "-" & clsDatum.Month & "-" & clsDatum.Year & " -odobren", 0))
            Catch ex As Exception
            End Try
        End While
        ldr.Close()

    End Sub

    Public Shared Sub Zahtjevi_TekuciLista(ByVal EmployeeID As Integer, ByRef zahtjeviLista As ArrayList, _
                           ByRef clsMySqlConnection As MySql.Data.MySqlClient.MySqlConnection)

        Dim lcmd As MySql.Data.MySqlClient.MySqlCommand
        Dim ldr As MySql.Data.MySqlClient.MySqlDataReader
        Dim strSQL As String = ""

        strSQL = "SELECT zahtjevi_id, datum FROM evd_zahtjevi t1 WHERE employeeID = " & EmployeeID & " AND zahtjevi_id  NOT IN " _
        & "(SELECT DISTINCT zahtjevi_id FROM evd_prisustva_zahtjevi WHERE employeeID = " & EmployeeID & " AND (evd_odobrio IS NOT NULL OR evd_kontrolisao IS NOT NULL) ORDER BY zahtjevi_id );"
        lcmd = New MySql.Data.MySqlClient.MySqlCommand(strSQL, clsMySqlConnection)

        Dim clsDatum As MySql.Data.Types.MySqlDateTime

        ldr = lcmd.ExecuteReader
        While ldr.Read
            Try
                clsDatum = ldr.Item("datum")
                zahtjeviLista.Add(New EvidencijaZahtjevi(ldr.Item("zahtjevi_id"), EmployeeID, "datum : " & clsDatum.Day & "-" & clsDatum.Month & "-" & clsDatum.Year & " - otvoren", -1))
            Catch ex As Exception
            End Try
        End While
        ldr.Close()

    End Sub

    Public Shared Sub Zahtjevi_Select_Sati(ByRef satiLista As ArrayList, ByVal datum As String, ByVal employeeID As Integer, ByRef odobrenjaLista As ArrayList, ByVal zahtjevID As Integer)


        Dim cnData As New MySql.Data.MySqlClient.MySqlConnection
        Dim lcmd As MySql.Data.MySqlClient.MySqlCommand
        Dim ldr As MySql.Data.MySqlClient.MySqlDataReader
        Dim rptTrajanje As String
        Dim rptDatum As String
        Dim evdPrisustvaID As Integer

        ' Dodaj u bazu evidenciju za ovaj dan
        cnData.ConnectionString = ConnectionString
        cnData.Open()

        Dim strSQL As String = "SELECT t1.evd_prisustva_id, t1.sifra_placanja,  DATE_FORMAT(t1.datum,'%d-%m-%Y') AS datum, DATE_FORMAT(t1.vrijeme_od, '%H:%i') AS vod, DATE_FORMAT(t1.vrijeme_do, '%H:%i') " _
                & " AS vdo, DATE_FORMAT(t1.vrijeme_ukupno, '%H:%i') AS ukupno, t2.naziv " _
                & " FROM evd_prisustva_zahtjevi t1 INNER JOIN sfr_placanja t2 on t1.sifra_placanja = t2.sifra WHERE employeeID=" & employeeID.ToString & " AND datum= '" & datum & "' AND zahtjevi_id =" & zahtjevID & ";"

        lcmd = New MySql.Data.MySqlClient.MySqlCommand(strSQL, cnData)
        ldr = lcmd.ExecuteReader
        While ldr.Read
            Try
                evdPrisustvaID = ldr.Item("evd_prisustva_id")
                'rptTrajanje = MySQL_ByteToString(ldr.Item("ukupno"))
                'rptDatum = MySQL_ByteToString(ldr.Item("datum"))
                ' TODO: Pretvaranje ByteStream u String datuma upitno!
                rptTrajanje = ldr.Item("ukupno")
                rptDatum = ldr.Item("datum")
            Catch ex As Exception
                rptTrajanje = ""
                rptDatum = ""
            End Try
            satiLista.Add(New EvidencijaDan(ldr.Item("sifra_placanja"), ldr.Item("naziv"), rptDatum, rptTrajanje))
        End While
        ldr.Close()

        strSQL = "SELECT CONCAT(t2.LastName,' ',t2.FirstName) AS EmpName FROM evd_prisustva_odobrenja t1, evd_employees t2 WHERE t1.evd_prisustva_id = " & evdPrisustvaID & " AND t1.employeeID= t2.EmployeeID ORDER BY t1.DummyTime;"
        lcmd.CommandText = strSQL
        ldr = lcmd.ExecuteReader
        While ldr.Read
            Try
                odobrenjaLista.Add(ldr.Item("EmpName"))
            Catch ex As Exception
            End Try

        End While
        ldr.Close()
        cnData.Close()

    End Sub

    Public Shared Function Zahtjevi_IsExistingSati(ByVal datum As String, ByVal employeeID As Integer, ByVal zahtjevID As Integer) As Boolean

        Dim cnData As New MySql.Data.MySqlClient.MySqlConnection
        Dim lcmd As MySql.Data.MySqlClient.MySqlCommand
        Dim ldr As MySql.Data.MySqlClient.MySqlDataReader

        Dim zahIsExisting As Boolean = False

        cnData.ConnectionString = ConnectionString
        cnData.Open()

        Dim strSQL As String = "SELECT t1.evd_prisustva_id, t1.sifra_placanja,  DATE_FORMAT(t1.datum,'%d-%m-%Y') AS datum, DATE_FORMAT(t1.vrijeme_od, '%H:%i') AS vod, DATE_FORMAT(t1.vrijeme_do, '%H:%i') " _
                & " AS vdo, DATE_FORMAT(t1.vrijeme_ukupno, '%H:%i') AS ukupno, t2.naziv " _
                & " FROM evd_prisustva_zahtjevi t1 INNER JOIN sfr_placanja t2 on t1.sifra_placanja = t2.sifra WHERE employeeID=" & employeeID.ToString & " AND datum= '" & datum & "' AND zahtjevi_id <>" & zahtjevID & ";"
        lcmd = New MySql.Data.MySqlClient.MySqlCommand(strSQL, cnData)
        ldr = lcmd.ExecuteReader
        If ldr.HasRows Then
            zahIsExisting = True
        End If
        ldr.Close()


        cnData.Close()
        Return zahIsExisting
    End Function

    Public Shared Function Zahtjevi_IsWritableSati(ByVal employeeID As Integer, ByVal zahtjevID As Integer) As Boolean

        Dim cnData As New MySql.Data.MySqlClient.MySqlConnection
        Dim lcmd As MySql.Data.MySqlClient.MySqlCommand
        Dim ldr As MySql.Data.MySqlClient.MySqlDataReader

        Dim zahIsExisting As Boolean = False

        cnData.ConnectionString = ConnectionString
        cnData.Open()
        Dim strSQL As String = "SELECT zahtjevi_id FROM evd_prisustva_zahtjevi  WHERE employeeID=" & employeeID.ToString & " AND zahtjevi_id =" & zahtjevID & " ;"
        lcmd = New MySql.Data.MySqlClient.MySqlCommand(strSQL, cnData)
        ldr = lcmd.ExecuteReader
        If ldr.HasRows Then
            ldr.Close()
            strSQL = "SELECT zahtjevi_id FROM evd_prisustva_zahtjevi  WHERE employeeID=" & employeeID.ToString & " AND zahtjevi_id =" & zahtjevID & " AND (evd_podnio IS NULL AND evd_odobrio IS NULL AND evd_kontrolisao IS NULL);"
            lcmd.CommandText = strSQL
            ldr = lcmd.ExecuteReader
            If ldr.HasRows Then
                zahIsExisting = True
            End If
        Else
            ldr.Close()
            strSQL = "SELECT zahtjevi_id FROM evd_zahtjevi  WHERE employeeID=" & employeeID.ToString & " AND zahtjevi_id =" & zahtjevID & " AND (evd_podnio IS NULL AND evd_odobrio IS NULL AND evd_kontrolisao IS NULL);"
            lcmd.CommandText = strSQL
            ldr = lcmd.ExecuteReader
            If ldr.HasRows Then
                zahIsExisting = True
            End If
        End If

        ldr.Close()

        cnData.Close()
        Return zahIsExisting
    End Function

    Public Shared Sub Zahtjevi_Insert_Sati(ByVal datum As String, ByVal employeeID As Integer, ByVal selVrstaPlacanja As String, ByVal zahtjevID As Integer)
        Dim cnData As New MySql.Data.MySqlClient.MySqlConnection
        Dim lcmd As MySql.Data.MySqlClient.MySqlCommand


        If Zahtjevi_IsExistingSati(datum, employeeID, zahtjevID) Then
            Exit Sub
        End If

        cnData.ConnectionString = ConnectionString
        cnData.Open()


        Dim insSQL As String = "INSERT INTO evd_prisustva_zahtjevi (employeeID, sifra_placanja, datum, vrijeme_ukupno, zahtjevi_id)	VALUES(" & employeeID.ToString & ", '" & selVrstaPlacanja & "','" & datum & "','08:00:00', " & zahtjevID & ");"
        Dim delSQL As String = "DELETE FROM evd_prisustva_zahtjevi WHERE datum = '" & datum & "' AND employeeID=" & employeeID.ToString & " AND zahtjevi_id=" & zahtjevID & ";"
        lcmd = New MySql.Data.MySqlClient.MySqlCommand(delSQL, cnData)
        ' lcmd.CommandText = insSQL
        Try
            If selVrstaPlacanja = "DELETE" Then
                lcmd.ExecuteNonQuery()
            Else
                lcmd.ExecuteNonQuery()
                lcmd.CommandText = insSQL
                lcmd.ExecuteNonQuery()
            End If
        Catch ex As Exception

        End Try

        cnData.Close()
    End Sub

    Public Shared Sub Zahtjevi_NoviZahtjev(ByVal EmployeeID As Integer, ByRef clsMySqlConnection As MySql.Data.MySqlClient.MySqlConnection)

        Dim lcmd As MySql.Data.MySqlClient.MySqlCommand
        Dim zahtjeviLista As New ArrayList
        Dim insSQL As String

        Zahtjevi_TekuciLista(EmployeeID, zahtjeviLista, clsMySqlConnection)
        If zahtjeviLista.Count > 0 Then
            Exit Sub

        Else

            insSQL = "INSERT INTO evd_zahtjevi (employeeID, datum) VALUES 	(" & EmployeeID.ToString & ", 	NOW() );"
            lcmd = New MySql.Data.MySqlClient.MySqlCommand(insSQL, clsMySqlConnection)

            Try
                lcmd.ExecuteNonQuery()
            Catch ex As Exception

            End Try

        End If

    End Sub

    Public Shared Sub Zahtjevi_Odobrenje(ByVal ZahtjevID As Integer, ByVal employeeID As Integer, _
                                           ByVal employeeID_odobrava As Integer, ByVal evd_kontrola As Boolean)

        Dim cnData As New MySql.Data.MySqlClient.MySqlConnection
        Dim lcmd As MySql.Data.MySqlClient.MySqlCommand
        Dim strSQL As String = ""

        Dim poruka As String = ""
        ' Dodaj u bazu evidenciju za ovaj dan
        cnData.ConnectionString = ConnectionString
        cnData.Open()

        Select Case evd_kontrola
            Case True
                strSQL = "UPDATE evd_prisustva_zahtjevi SET evd_kontrolisao = " & employeeID_odobrava & " WHERE employeeID=" & employeeID & " AND evd_podnio IS NOT NULL AND evd_odobrio IS NOT NULL AND zahtjevi_id=" & ZahtjevID & ";"
            Case False
                If employeeID = employeeID_odobrava Then
                    strSQL = "UPDATE evd_prisustva_zahtjevi SET evd_podnio = " & employeeID_odobrava & " WHERE  employeeID=" & employeeID & " AND zahtjevi_id=" & ZahtjevID & ";"
                ElseIf employeeID <> employeeID_odobrava Then
                    strSQL = "UPDATE evd_prisustva_zahtjevi SET evd_odobrio = " & employeeID_odobrava & " WHERE  employeeID=" & employeeID & " AND evd_podnio IS NOT NULL  AND zahtjevi_id=" & ZahtjevID & ";"
                End If

        End Select



        lcmd = New MySql.Data.MySqlClient.MySqlCommand(strSQL, cnData)

        Try
            lcmd.ExecuteNonQuery()
            If evd_kontrola Then
                Zahtjevi_PrepisiUEvidencije(ZahtjevID, poruka)
            End If
        Catch ex As Exception

        End Try


        cnData.Close()
    End Sub

    Public Shared Sub Zahtjevi_OdobrenjeByRole(ByVal ZahtjevID As Integer, ByVal employeeID As Integer, _
                                           ByVal employeeID_odobrava As Integer, ByVal RoleName As String)

        Dim cnData As New MySql.Data.MySqlClient.MySqlConnection
        Dim lcmd As MySql.Data.MySqlClient.MySqlCommand
        Dim strSQL As String = ""

        Dim poruka As String = ""
        ' Dodaj u bazu evidenciju za ovaj dan
        cnData.ConnectionString = ConnectionString
        cnData.Open()

        Select Case RoleName

            Case "pravna"

            Case "evidencija"
                strSQL = "UPDATE evd_prisustva_zahtjevi SET evd_kontrolisao = " & employeeID_odobrava & " WHERE employeeID=" & employeeID & " AND evd_podnio IS NOT NULL AND evd_odobrio IS NOT NULL AND zahtjevi_id=" & ZahtjevID & ";"
                lcmd = New MySql.Data.MySqlClient.MySqlCommand(strSQL, cnData)

                Try
                    lcmd.ExecuteNonQuery()
                Catch ex As Exception
                End Try
                Zahtjevi_PrepisiUEvidencije(ZahtjevID, poruka)

            Case "rukovodilac"
                strSQL = "UPDATE evd_prisustva_zahtjevi SET evd_odobrio = " & employeeID_odobrava & " WHERE  employeeID=" & employeeID & " AND evd_podnio IS NOT NULL  AND zahtjevi_id=" & ZahtjevID & ";"
                lcmd = New MySql.Data.MySqlClient.MySqlCommand(strSQL, cnData)

                Try
                    lcmd.ExecuteNonQuery()
                Catch ex As Exception
                End Try

            Case "uposlenik"
                strSQL = "UPDATE evd_prisustva_zahtjevi SET evd_podnio = " & employeeID_odobrava & " WHERE  employeeID=" & employeeID & " AND zahtjevi_id=" & ZahtjevID & ";"
                lcmd = New MySql.Data.MySqlClient.MySqlCommand(strSQL, cnData)

                Try
                    lcmd.ExecuteNonQuery()
                Catch ex As Exception
                End Try

        End Select

        cnData.Close()
    End Sub

    Public Shared Sub Zahtjevi_OdobrenjeByRole(ByVal employeeID As Integer, _
                                       ByVal employeeID_odobrava As Integer, ByVal RoleName As String)

        Dim cnData As New MySql.Data.MySqlClient.MySqlConnection
        Dim lcmd As MySql.Data.MySqlClient.MySqlCommand
        Dim strSQL As String = ""

        Dim poruka As String = ""
        ' Dodaj u bazu evidenciju za ovaj dan
        cnData.ConnectionString = ConnectionString
        cnData.Open()

        Select Case RoleName

            Case "pravna"

            Case "evidencija"
                strSQL = "UPDATE evd_prisustva_zahtjevi SET evd_kontrolisao = " & employeeID_odobrava & " WHERE employeeID=" & employeeID & " AND evd_podnio IS NOT NULL AND evd_odobrio IS NOT NULL ;"
                lcmd = New MySql.Data.MySqlClient.MySqlCommand(strSQL, cnData)

                Try
                    lcmd.ExecuteNonQuery()
                Catch ex As Exception
                End Try
                'Zahtjevi_PrepisiUEvidencije(ZahtjevID, poruka)

            Case "rukovodilac"
                strSQL = "UPDATE evd_prisustva_zahtjevi SET evd_odobrio = " & employeeID_odobrava & " WHERE  employeeID=" & employeeID & " AND evd_podnio IS NOT NULL ;"
                lcmd = New MySql.Data.MySqlClient.MySqlCommand(strSQL, cnData)

                Try
                    lcmd.ExecuteNonQuery()
                Catch ex As Exception
                End Try

            Case "uprava"
                strSQL = "UPDATE evd_prisustva_zahtjevi SET evd_odobrio = " & employeeID_odobrava & " WHERE  employeeID=" & employeeID & " AND evd_podnio IS NOT NULL ;"
                lcmd = New MySql.Data.MySqlClient.MySqlCommand(strSQL, cnData)

                Try
                    lcmd.ExecuteNonQuery()
                Catch ex As Exception
                End Try

            Case "uposlenik"
                strSQL = "UPDATE evd_prisustva_zahtjevi SET evd_podnio = " & employeeID_odobrava & " WHERE  employeeID=" & employeeID & "  AND evd_podnio IS NULL ;"
                lcmd = New MySql.Data.MySqlClient.MySqlCommand(strSQL, cnData)

                Try
                    lcmd.ExecuteNonQuery()
                Catch ex As Exception
                End Try

        End Select

        cnData.Close()
    End Sub

    Public Shared Sub Zahtjevi_OdobrenjePonisti(ByVal ZahtjevID As Integer, ByVal employeeID As Integer, _
                                           ByVal employeeID_odobrava As Integer, ByVal evd_kontrola As Boolean)
        Dim cnData As New MySql.Data.MySqlClient.MySqlConnection
        Dim lcmd As MySql.Data.MySqlClient.MySqlCommand
        Dim strSQL As String = ""


        Dim poruka As String = ""

        cnData.ConnectionString = ConnectionString
        cnData.Open()

        Select Case evd_kontrola
            Case True
                strSQL = "UPDATE evd_prisustva_zahtjevi SET evd_kontrolisao = NULL WHERE employeeID=" & employeeID & " AND zahtjevi_id=" & ZahtjevID & ";"
            Case False
                If employeeID = employeeID_odobrava Then
                    strSQL = "UPDATE evd_prisustva_zahtjevi SET evd_podnio = NULL WHERE  employeeID=" & employeeID & " AND evd_odobrio IS NULL  AND zahtjevi_id=" & ZahtjevID & ";"
                ElseIf employeeID <> employeeID_odobrava Then
                    strSQL = "UPDATE evd_prisustva_zahtjevi SET evd_odobrio = NULL WHERE employeeID=" & employeeID & " AND evd_kontrolisao IS NULL  AND zahtjevi_id=" & ZahtjevID & ";"
                End If

        End Select



        lcmd = New MySql.Data.MySqlClient.MySqlCommand(strSQL, cnData)

        Try
            lcmd.ExecuteNonQuery()
            If evd_kontrola Then
                Zahtjevi_ObrisiIzEvidencije(ZahtjevID, poruka)
            End If
        Catch ex As Exception

        End Try


        cnData.Close()

    End Sub

    Public Shared Sub Zahtjevi_PrikaziOdobrenja(ByRef lbl1 As String, ByRef lbl2 As String, ByRef lbl3 As String, ByVal current_employee As Integer, ByVal current_zahtjev As Integer)
        lbl1 = "Nije podnesena!"
        lbl2 = "Nije odobrena!"
        lbl3 = "Nije kontrolisana!"

        Dim cnData As New MySql.Data.MySqlClient.MySqlConnection
        Dim lcmd As MySql.Data.MySqlClient.MySqlCommand
        Dim ldr As MySql.Data.MySqlClient.MySqlDataReader

        ' Dodaj u bazu evidenciju za ovaj dan
        cnData.ConnectionString = ConnectionString
        cnData.Open()

        Dim strSQL As String = "SELECT DISTINCT t1.evd_podnio, e1.LastName, e1.FirstName FROM evd_prisustva_zahtjevi t1 " _
                   & "INNER JOIN evd_employees e1 ON t1.evd_podnio = e1.EmployeeID WHERE t1.zahtjevi_id = " & current_zahtjev & " AND t1.employeeID = " & current_employee & ";"

        lcmd = New MySql.Data.MySqlClient.MySqlCommand(strSQL, cnData)
        ldr = lcmd.ExecuteReader
        While ldr.Read
            Try
                lbl1 = ldr.Item("LastName") & " " & ldr.Item("FirstName")
            Catch ex As Exception
                lbl1 = "Greška prilikom oèitavanja baze!"
            End Try

        End While
        ldr.Close()

        strSQL = "SELECT DISTINCT t1.evd_podnio, e1.LastName, e1.FirstName FROM evd_prisustva_zahtjevi t1 " _
                   & "INNER JOIN evd_employees e1 ON t1.evd_odobrio = e1.EmployeeID WHERE t1.zahtjevi_id = " & current_zahtjev & " AND t1.employeeID = " & current_employee & ";"

        lcmd.CommandText = strSQL
        ldr = lcmd.ExecuteReader
        While ldr.Read
            Try
                lbl2 = ldr.Item("LastName") & " " & ldr.Item("FirstName")
            Catch ex As Exception
                lbl2 = "Greška prilikom oèitavanja baze!"
            End Try

        End While
        ldr.Close()

        strSQL = "SELECT DISTINCT t1.evd_podnio, e1.LastName, e1.FirstName FROM evd_prisustva_zahtjevi t1 " _
                   & "INNER JOIN evd_employees e1 ON t1.evd_kontrolisao = e1.EmployeeID WHERE t1.zahtjevi_id = " & current_zahtjev & "  AND t1.employeeID = " & current_employee & ";"

        lcmd.CommandText = strSQL
        ldr = lcmd.ExecuteReader
        While ldr.Read
            Try
                lbl3 = ldr.Item("LastName") & " " & ldr.Item("FirstName")
            Catch ex As Exception
                lbl3 = "Greška prilikom oèitavanja baze!"
            End Try

        End While
        ldr.Close()
        cnData.Close()

    End Sub

    Public Shared Sub Zahtjevi_PrikaziOdobrenja(ByRef lbl1 As String, ByRef lbl2 As String, ByRef lbl3 As String, ByVal current_employee As Integer)
        lbl1 = "Nije podnesena!"
        lbl2 = "Nije odobrena!"
        lbl3 = "Nije kontrolisana!"

        Dim cnData As New MySql.Data.MySqlClient.MySqlConnection
        Dim lcmd As MySql.Data.MySqlClient.MySqlCommand
        Dim ldr As MySql.Data.MySqlClient.MySqlDataReader

        ' Dodaj u bazu evidenciju za ovaj dan
        cnData.ConnectionString = ConnectionString
        cnData.Open()

        Dim strSQL As String = "SELECT DISTINCT t1.evd_podnio, e1.LastName, e1.FirstName FROM evd_prisustva_zahtjevi t1 " _
                   & "INNER JOIN evd_employees e1 ON t1.evd_podnio = e1.EmployeeID WHERE t1.employeeID = " & current_employee & ";"

        lcmd = New MySql.Data.MySqlClient.MySqlCommand(strSQL, cnData)
        ldr = lcmd.ExecuteReader
        While ldr.Read
            Try
                lbl1 = ldr.Item("LastName") & " " & ldr.Item("FirstName")
            Catch ex As Exception
                lbl1 = "Greška prilikom oèitavanja baze!"
            End Try

        End While
        ldr.Close()

        strSQL = "SELECT DISTINCT t1.evd_podnio, e1.LastName, e1.FirstName FROM evd_prisustva_zahtjevi t1 " _
                   & "INNER JOIN evd_employees e1 ON t1.evd_odobrio = e1.EmployeeID WHERE t1.employeeID = " & current_employee & ";"

        lcmd.CommandText = strSQL
        ldr = lcmd.ExecuteReader
        While ldr.Read
            Try
                lbl2 = ldr.Item("LastName") & " " & ldr.Item("FirstName")
            Catch ex As Exception
                lbl2 = "Greška prilikom oèitavanja baze!"
            End Try

        End While
        ldr.Close()

        strSQL = "SELECT DISTINCT t1.evd_podnio, e1.LastName, e1.FirstName FROM evd_prisustva_zahtjevi t1 " _
                   & "INNER JOIN evd_employees e1 ON t1.evd_kontrolisao = e1.EmployeeID WHERE t1.employeeID = " & current_employee & ";"

        lcmd.CommandText = strSQL
        ldr = lcmd.ExecuteReader
        While ldr.Read
            Try
                lbl3 = ldr.Item("LastName") & " " & ldr.Item("FirstName")
            Catch ex As Exception
                lbl3 = "Greška prilikom oèitavanja baze!"
            End Try

        End While
        ldr.Close()
        cnData.Close()

    End Sub

    Public Shared Sub Zahtjevi_OdobrenjePonistiByRole(ByVal ZahtjevID As Integer, ByVal employeeID As Integer, _
                                           ByVal employeeID_odobrava As Integer, ByVal RoleName As String)
        Dim cnData As New MySql.Data.MySqlClient.MySqlConnection
        Dim lcmd As MySql.Data.MySqlClient.MySqlCommand
        Dim strSQL As String = ""


        Dim poruka As String = ""

        cnData.ConnectionString = ConnectionString
        cnData.Open()


        Select Case RoleName


            Case "pravna"

            Case "evidencija"
                strSQL = "UPDATE evd_prisustva_zahtjevi SET evd_kontrolisao = NULL WHERE employeeID=" & employeeID & " AND zahtjevi_id=" & ZahtjevID & ";"
                lcmd = New MySql.Data.MySqlClient.MySqlCommand(strSQL, cnData)

                Try
                    lcmd.ExecuteNonQuery()
                Catch ex As Exception
                End Try
                Zahtjevi_ObrisiIzEvidencije(ZahtjevID, poruka)

            Case "rukovodilac"
                strSQL = "UPDATE evd_prisustva_zahtjevi SET evd_odobrio = NULL WHERE employeeID=" & employeeID & " AND zahtjevi_id=" & ZahtjevID & " AND evd_kontrolisao IS NULL;"
                lcmd = New MySql.Data.MySqlClient.MySqlCommand(strSQL, cnData)

                Try
                    lcmd.ExecuteNonQuery()
                Catch ex As Exception
                End Try

            Case "uposlenik"
                strSQL = "UPDATE evd_prisustva_zahtjevi SET evd_podnio = NULL WHERE  employeeID=" & employeeID & " AND evd_odobrio IS NULL  AND zahtjevi_id=" & ZahtjevID & ";"
                lcmd = New MySql.Data.MySqlClient.MySqlCommand(strSQL, cnData)

                Try
                    lcmd.ExecuteNonQuery()
                Catch ex As Exception
                End Try
        End Select

        cnData.Close()

    End Sub

    Public Shared Sub Zahtjevi_PrepisiUEvidencije(ByVal ZahtjevID As Integer, ByRef poruka As String)

        Dim cnData As New MySql.Data.MySqlClient.MySqlConnection
        Dim lcmd As MySql.Data.MySqlClient.MySqlCommand
        Dim strSQL As String = ""
        ' Dodaj u bazu evidenciju za ovaj dan
        cnData.ConnectionString = ConnectionString
        cnData.Open()
        ' Prepisi dane zahtjeva u tabelu

        ' Prepisi iz tabele u evidnecije u ovisnosit od pravila

        ' Evidencija postoji - abort aktivnosti
        ' azuriraj poruku

        ' Evidencija ne postoji - prepisi i potvrdi
        ' azuriraj poruku

        If EvidencijaExistswhereZahtjev(ZahtjevID) Then
            poruka = "Vec postoje obiljezeni dani u evidencijama!"
            Exit Sub
        End If


        strSQL = "INSERT INTO evd_prisustva (employeeID, sifra_placanja, datum, vrijeme_ukupno, evd_podnio, evd_odobrio, evd_kontrolisao, evd_locked, evd_zahtjev_id)" _
                & " SELECT 	employeeID, 'P20', datum, '08:00:00',NULL, NULL, NULL, NULL, zahtjevi_id FROM evd_prisustva_zahtjevi WHERE zahtjevi_id=" & ZahtjevID & ";"

        lcmd = New MySql.Data.MySqlClient.MySqlCommand(strSQL, cnData)

        Try
            lcmd.ExecuteNonQuery()
            poruka = "Zahtjev uspjesno evidentiran u evidencijama!"
        Catch ex As Exception

        End Try


        cnData.Close()
    End Sub

    Public Shared Sub Zahtjevi_ObrisiIzEvidencije(ByVal ZahtjevID As Integer, ByRef poruka As String)

        Dim cnData As New MySql.Data.MySqlClient.MySqlConnection
        Dim lcmd As MySql.Data.MySqlClient.MySqlCommand
        Dim strSQL As String = ""
        ' Dodaj u bazu evidenciju za ovaj dan
        cnData.ConnectionString = ConnectionString
        cnData.Open()


        strSQL = "DELETE FROM evd_prisustva WHERE datum IN(SELECT 	datum FROM evd_prisustva_zahtjevi WHERE zahtjevi_id=" & ZahtjevID & ") " _
     & " AND employeeID = (SELECT DISTINCT employeeID FROM evd_prisustva_zahtjevi WHERE zahtjevi_id=" & ZahtjevID & ") ;"

        lcmd = New MySql.Data.MySqlClient.MySqlCommand(strSQL, cnData)

        Try
            lcmd.ExecuteNonQuery()
            poruka = "Zahtjev uspjesno obrisan iz evidencija!"
        Catch ex As Exception

        End Try


        cnData.Close()
    End Sub

    Public Shared Function EvidencijaExistswhereZahtjev(ByVal ZahtjevID As Integer) As Boolean
        Dim cnData As New MySql.Data.MySqlClient.MySqlConnection
        Dim lcmd As MySql.Data.MySqlClient.MySqlCommand
        Dim strSQL As String = ""
        Dim lrd As MySql.Data.MySqlClient.MySqlDataReader

        cnData.ConnectionString = ConnectionString
        cnData.Open()

        Dim retValue As Boolean = False

        strSQL = "SELECT 	datum FROM evd_prisustva WHERE datum IN(SELECT 	datum FROM evd_prisustva_zahtjevi WHERE zahtjevi_id=" & ZahtjevID & ") " _
            & " AND employeeID = (SELECT DISTINCT employeeID FROM evd_prisustva_zahtjevi WHERE zahtjevi_id=" & ZahtjevID & ") ;"

        Try
            lcmd = New MySql.Data.MySqlClient.MySqlCommand(strSQL, cnData)
            lrd = lcmd.ExecuteReader
            retValue = lrd.HasRows
            lrd.Close()
        Catch ex As Exception

        End Try

        cnData.Close()
        Return (retValue)

    End Function


#End Region


#Region "**** Funkcije ****"


    Public Shared Function Date_MySQLFormat(ByVal webDate As DateTime) As String
        Dim format As String = "yyyy/MM/dd"
        Dim retVal As String = "1900/01/01"
        'format = "yyyy-MM-dd"

        Try
            retVal = webDate.ToString(format)

        Catch ex As Exception
            'retVal = ""
        End Try

        Return retVal
    End Function

    Public Shared Function MySQL_ByteToString(ByRef dBytes As Byte()) As String
        Dim enc As New System.Text.ASCIIEncoding()
        Dim tmpString As String

        Try
            tmpString = enc.GetString(dBytes)
        Catch ex As Exception
            tmpString = ""
        End Try

        Return tmpString

    End Function

    Public Shared Function MySQL_ByteToString(ByRef dBytes As String) As String
        Dim enc As New System.Text.ASCIIEncoding()
        Dim tmpString As String

        Try
            tmpString = dBytes
        Catch ex As Exception
            tmpString = ""
        End Try

        Return tmpString

    End Function

#End Region


#Region "**** Klase ****"



    Public Class EvidencijaDan
        Private mySifra As String
        Private myOpis As String
        Private myDatum As String
        Private myTime As String

        Public Sub New(ByVal newsifra As String, ByVal newOpis As String, ByVal newDatum As String, ByVal newTime As String)
            mySifra = newsifra
            myOpis = newOpis
            myDatum = newDatum
            myTime = newTime
        End Sub

        Public ReadOnly Property Sifra() As String
            Get
                Return mySifra
            End Get
        End Property

        Public ReadOnly Property Opis() As String
            Get
                Return myOpis
            End Get
        End Property
        Public ReadOnly Property Datum() As String
            Get
                Return myDatum
            End Get
        End Property
        Public ReadOnly Property Trajanje() As String
            Get
                Return myTime
            End Get
        End Property
    End Class

    Public Class EvidencijaEmployees
        Private myEmployeeID As Integer
        Private myLevel As Integer
        Private myName As String


        Public Sub New(ByVal newsifra As Integer, ByVal newLevel As Integer, ByVal newName As String)
            myEmployeeID = newsifra
            myLevel = newLevel
            myName = newName
        End Sub

        Public ReadOnly Property EmployeeID() As Integer
            Get
                Return myEmployeeID
            End Get
        End Property

        Public ReadOnly Property Level() As Integer
            Get
                Return myLevel
            End Get
        End Property
        Public ReadOnly Property Name() As String
            Get
                Return myName
            End Get
        End Property
    End Class

    Public Class EvidencijaZahtjevi
        Private myZahtjevID As Integer
        Private myEmployeeID As Integer
        Private myDatum As String
        Private myStatus As Integer


        Public Sub New(ByVal newzahtjev As Integer, ByVal newemployeeid As Integer, ByVal newDatum As String, ByVal newStatus As Integer)
            myZahtjevID = newzahtjev
            myEmployeeID = newemployeeid
            myDatum = newDatum
            myStatus = newStatus
        End Sub

        Public ReadOnly Property ZahtjevID() As Integer
            Get
                Return myZahtjevID
            End Get
        End Property

        Public ReadOnly Property EmployeeID() As Integer
            Get
                Return myEmployeeID
            End Get
        End Property

        Public ReadOnly Property Datum() As String
            Get
                Return myDatum
            End Get
        End Property

        Public ReadOnly Property Status() As Integer
            Get
                Return myStatus
            End Get
        End Property
    End Class


#End Region

#Region "**** Forme ****"

    Public Shared Function isAllowed_Bolovanje() As Boolean
        Dim con As New MySqlClient.MySqlConnection(ConnectionString)

        Dim sqlText As String = ""

        Dim cmd As New MySqlClient.MySqlCommand(sqlText, con)

        con.Open()
        Dim rd As MySqlClient.MySqlDataReader


        sqlText = "SELECT adm_value AS BOLOVANJE_CONTROLSENABLED FROM  adm_configuration WHERE adm_parameter = 'BOLOVANJE_CONTROLSENABLED';"
        cmd.CommandText = sqlText

        Dim _Parametar As Integer
        Try
            rd = cmd.ExecuteReader
            While rd.Read
                _Parametar = CBool(CInt(rd.Item("BOLOVANJE_CONTROLSENABLED")))
            End While
            rd.Close()
        Catch ex As Exception

        End Try

        con.Close()

        Return _Parametar
    End Function

#End Region

End Class
