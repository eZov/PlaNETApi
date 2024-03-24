Imports Microsoft.VisualBasic
Imports System
Imports System.Data
Imports System.Configuration
Imports System.Web
Imports System.Web.Security
Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports System.Web.UI.WebControls.WebParts
Imports System.Web.UI.HtmlControls
Imports System.Data.OleDb
Imports System.Data.SqlClient
Imports MySql.Data
Imports System.Collections


Public Class ClsUposleniPodaci
    Private cnstr As String = ConfigurationManager.ConnectionStrings("Evidencije").ConnectionString
    Private sql As String

    '
    ' TODO: Add constructor logic here
    '
    Public Sub New()
    End Sub

    Public Sub Insert(ByVal CustomerName As String, ByVal Gender As String, ByVal City As String, ByVal State As String, ByVal CustomerType As String)
        Dim sql As String = "Insert Into Customer (Name, Gender, City, State, Type) Values ('" & CustomerName & "' , '" & Gender & "', '" & City & "', '" & State & "', '" & CustomerType & "')"
        sqlExecute(sql)
    End Sub

    Public Function Fetch(Optional ByVal EmployeeID As Integer = 0) As DataTable
        Dim sql As String = "SELECT * FROM evd_employees WHERE EmployeeID=" & EmployeeID & " ;"
        Dim da As New MySqlClient.MySqlDataAdapter(sql, cnstr)
        Dim dt As New DataTable()
        da.Fill(dt)
        Return dt
    End Function

    Public Sub Update(ByVal Iznos_Sati As Double, ByVal id As Integer)
        Dim sql As String = "UPDATE `xfc_uposleni-placanja` SET Iznos_Sati = " & Iznos_Sati & " WHERE id=" & id & ";"
        sqlExecute(sql)
    End Sub

    Public Sub Delete(ByVal CustomerCode As Integer)
        Dim sql As String = "Delete Customer Where Code=" & CustomerCode
        sqlExecute(sql)
    End Sub

    Public Function FetchCustomerType() As DataTable
        Dim sql As String = "Select Distinct Type From Customer"
        Dim da As New MySqlClient.MySqlDataAdapter(sql, cnstr)
        Dim dt As New DataTable()
        da.Fill(dt)
        Return dt
    End Function

    Public Sub sqlExecute(ByVal sql As String)
        Dim conn As New MySqlClient.MySqlConnection(cnstr)
        conn.Open()
        Try
            Dim cmd As New MySqlClient.MySqlCommand(sql, conn)
            cmd.ExecuteNonQuery()
        Catch ex As Exception

        End Try
        conn.Close()
        conn.Dispose()
    End Sub

    Public Sub InsertToDB(ByRef _Parameters As Hashtable, Optional ByRef EmployeeID As Integer = -1)
        Dim sqlText As String = "INSERT INTO evd_employees(EmployeeNumber, JMB, FirstName, LastName, BrSocOsig, Opc_Stanovanja, Opc_Porodice, " _
                                & "Opc_PIO, Opc_ZDR, Opc_ZPS, Dat_Zaposl, Staz_Prethodni) " _
                                & "VALUES(?EmployeeNumber, ?JMB,?FirstName,?LastName,?BrSocOsig,?Opc_Stanovanja,?Opc_Porodice," _
                                & "?Opc_PIO,?Opc_ZDR, ?Opc_ZPS, ?Dat_Zaposl, ?Staz_Prethodni );"

        Dim conn As New MySqlClient.MySqlConnection(cnstr)
        Dim cmd As MySqlClient.MySqlCommand
        conn.Open()

        Dim dbDatum As Date
        Dim dbStaz As Decimal

        Try
            cmd = conn.CreateCommand
            cmd.CommandText = sqlText
            cmd.Parameters.Add(New MySqlClient.MySqlParameter("?EmployeeID", _Parameters.Item("?EmployeeID")))
            cmd.Parameters.Add(New MySqlClient.MySqlParameter("?EmployeeNumber", _Parameters.Item("?EmployeeNumber")))
            cmd.Parameters.Add(New MySqlClient.MySqlParameter("?JMB", _Parameters.Item("?JMB")))
            cmd.Parameters.Add(New MySqlClient.MySqlParameter("?FirstName", _Parameters.Item("?FirstName")))
            cmd.Parameters.Add(New MySqlClient.MySqlParameter("?LastName", _Parameters.Item("?LastName")))
            cmd.Parameters.Add(New MySqlClient.MySqlParameter("?BrSocOsig", _Parameters.Item("?BrSocOsig")))
            cmd.Parameters.Add(New MySqlClient.MySqlParameter("?Opc_Stanovanja", _Parameters.Item("?Opc_Stanovanja")))
            cmd.Parameters.Add(New MySqlClient.MySqlParameter("?Opc_Porodice", _Parameters.Item("?Opc_Porodice")))
            cmd.Parameters.Add(New MySqlClient.MySqlParameter("?Opc_PIO", _Parameters.Item("?Opc_PIO")))
            cmd.Parameters.Add(New MySqlClient.MySqlParameter("?Opc_ZDR", _Parameters.Item("?Opc_ZDR")))
            cmd.Parameters.Add(New MySqlClient.MySqlParameter("?Opc_ZPS", _Parameters.Item("?Opc_ZPS")))

            ConvertStringToDatum(_Parameters.Item("dbDatum"), dbDatum)
            cmd.Parameters.Add(New MySqlClient.MySqlParameter("?Dat_Zaposl", dbDatum))

            ConvertDatumToStaz(dbStaz, _Parameters.Item("dbGod"), _Parameters.Item("dbMje"), _Parameters.Item("dbDani"))
            cmd.Parameters.Add(New MySqlClient.MySqlParameter("?Staz_Prethodni", dbStaz))

            cmd.ExecuteNonQuery()
        Catch ex As Exception

        End Try
        EmployeeID = Get_LAST_INSERT_ID_FromDB(conn)
        conn.Close()
        conn.Dispose()
    End Sub

    Public Function Get_LAST_INSERT_ID_FromDB(ByRef conn As MySqlClient.MySqlConnection) As Integer
        Dim lcmd As MySql.Data.MySqlClient.MySqlCommand
        Dim ldr As MySql.Data.MySqlClient.MySqlDataReader
        Dim strSQL As String


        strSQL = "SELECT LAST_INSERT_ID() As last_Id;"
        lcmd = New MySql.Data.MySqlClient.MySqlCommand(strSQL, conn)


        ldr = lcmd.ExecuteReader
        While ldr.Read
            Try

                Return ldr.Item("last_Id")

            Catch ex As Exception
            End Try
        End While
        ldr.Close()


        Return -1

    End Function

    Public Function Get_INSERT_USERID_FromDB(ByVal newUserName As String) As Integer

        Dim conn As New MySqlClient.MySqlConnection(cnstr)
        Dim lcmd As MySql.Data.MySqlClient.MySqlCommand
        Dim ldr As MySql.Data.MySqlClient.MySqlDataReader
        Dim strSQL As String

        conn.Open()
        strSQL = "SELECT id FROM my_aspnet_users WHERE NAME='" & newUserName & "' AND applicationId = 3;"
        lcmd = New MySql.Data.MySqlClient.MySqlCommand(strSQL, conn)


        ldr = lcmd.ExecuteReader
        While ldr.Read
            Try

                Return ldr.Item("id")

            Catch ex As Exception
            End Try
        End While
        ldr.Close()


        Return -1

    End Function

    Public Sub Insert_UserToEmployee_ToDB(ByVal _User_Id As Integer, ByVal _EmployeeID As Integer)
        Dim sqlText As String = "INSERT INTO my_aspnet_users_to_employees (users_id, employees_id )VALUES(?users_id, ?employees_id);"

        Dim conn As New MySqlClient.MySqlConnection(cnstr)
        Dim cmd As MySqlClient.MySqlCommand
        conn.Open()

        Try
            cmd = conn.CreateCommand
            cmd.CommandText = sqlText
            cmd.Parameters.Add(New MySqlClient.MySqlParameter("?users_id", _User_Id))
            cmd.Parameters.Add(New MySqlClient.MySqlParameter("?employees_id", _EmployeeID))

            cmd.ExecuteNonQuery()
        Catch ex As Exception

        End Try
        conn.Close()
        conn.Dispose()
    End Sub

    Public Sub Insert_EpoyeesEXT_ToDB(ByRef EmployeeID As Integer)
        Dim sqlText As String = "INSERT INTO evd_employees_ext (EmployeeID, Staz_G, Staz_M, Staz_D, RadnoMjestoSifra)	VALUES	( ?EmployeeID, '0', '0', '0',  '');"

        Dim conn As New MySqlClient.MySqlConnection(cnstr)
        Dim cmd As MySqlClient.MySqlCommand
        conn.Open()


        Try
            cmd = conn.CreateCommand
            cmd.CommandText = sqlText
            cmd.Parameters.Add(New MySqlClient.MySqlParameter("?EmployeeID", EmployeeID))
            cmd.ExecuteNonQuery()
        Catch ex As Exception

        End Try

        conn.Close()
        conn.Dispose()
    End Sub

    Public Sub Insert_EmployeesLPD_ToDB(ByRef EmployeeID As Integer, ByRef JMB As String)
        Dim sqlText As String = "INSERT INTO evd_employees_lpd " _
                             & "(EmployeeID, JMB, ImeRoditelja, Broj_LK, Mjesto_LK, 	Opcina_LK, Pol, " _
                             & "Mjesto_Rodjenja, Opcina_Rodjenja, Drzava_Rodjenja, Drzavljanstvo, Narod, BracnoStanje, " _
                             & "Supruznik, Djeca, KrvnaGrupa, KontaktOsoba, Pripravnik, StatusZaposlenja, DjecijiDodatak, " _
                             & "TerenskiDodatak, BrojClanovaDomacinstva, StrucnaSprema, NazivZavrseneSkole, NazivTekuceSkole, " _
                             & "Zanimanje, StrucniIspit, SpecijalistickaObuka, 	PosebnaZnanja, PoznavanjeRacunara, OcjenaRada, " _
                             & "PovredaRadneDiscipline, Invalidnost, SamohraniRoditelj, Adresa, Tel_pos, Tel_prv, Mob_prv, Email_pos, adresa_br	)" _
                             & "VALUES	(?EmployeeID, ?JMB, '', '', '', '', '',  " _
                                & "    '', '', '', 'o', '', '',  " _
                                & "    '', '', '', '', '', '', '', " _
                                & "    '', 0, '', '', '', " _
                                & "    '', '', '', '', '', '', " _
                                & "    '', '', '', '', '', '', '', '', '');"

        Dim conn As New MySqlClient.MySqlConnection(cnstr)
        Dim cmd As MySqlClient.MySqlCommand
        conn.Open()


        Try
            cmd = conn.CreateCommand
            cmd.CommandText = sqlText
            cmd.Parameters.Add(New MySqlClient.MySqlParameter("?EmployeeID", EmployeeID))
            cmd.Parameters.Add(New MySqlClient.MySqlParameter("?JMB", JMB))
            cmd.ExecuteNonQuery()
        Catch ex As Exception

        End Try

        conn.Close()
        conn.Dispose()
    End Sub

    Public Sub SaveToDB(ByRef _Parameters As Hashtable)
        Dim sqlText As String = "UPDATE evd_employees SET EmployeeNumber = ?EmployeeNumber , JMB = ?JMB , FirstName = ?FirstName , " _
                                & "LastName = ?LastName , BrSocOsig = ?BrSocOsig , " _
                                & " Opc_Stanovanja = ?Opc_Stanovanja , Opc_Porodice = ?Opc_Porodice , Opc_PIO = ?Opc_PIO , " _
                                & " Opc_ZDR = ?Opc_ZDR , Opc_ZPS = ?Opc_ZPS, DepartmentUP = ?Organizacija, Dat_Zaposl=?Dat_Zaposl, Staz_Prethodni = ?Staz_Prethodni,  RadnoMjesto = ?RadnoMjesto, Aktivan = ?Aktivan WHERE EmployeeID = ?EmployeeID;"

        Dim conn As New MySqlClient.MySqlConnection(cnstr)
        Dim cmd As MySqlClient.MySqlCommand
        Dim ldr As MySql.Data.MySqlClient.MySqlDataReader

        conn.Open()

        Dim dbDatum As Date
        Dim dbStaz As Decimal

        Try
            cmd = conn.CreateCommand
            cmd.CommandText = sqlText
            cmd.Parameters.Add(New MySqlClient.MySqlParameter("?EmployeeID", _Parameters.Item("?EmployeeID")))
            cmd.Parameters.Add(New MySqlClient.MySqlParameter("?EmployeeNumber", _Parameters.Item("?EmployeeNumber")))
            cmd.Parameters.Add(New MySqlClient.MySqlParameter("?JMB", _Parameters.Item("?JMB")))
            cmd.Parameters.Add(New MySqlClient.MySqlParameter("?FirstName", _Parameters.Item("?FirstName")))
            cmd.Parameters.Add(New MySqlClient.MySqlParameter("?LastName", _Parameters.Item("?LastName")))
            cmd.Parameters.Add(New MySqlClient.MySqlParameter("?BrSocOsig", _Parameters.Item("?BrSocOsig")))
            cmd.Parameters.Add(New MySqlClient.MySqlParameter("?Opc_Stanovanja", _Parameters.Item("?Opc_Stanovanja")))
            cmd.Parameters.Add(New MySqlClient.MySqlParameter("?Opc_Porodice", _Parameters.Item("?Opc_Porodice")))
            cmd.Parameters.Add(New MySqlClient.MySqlParameter("?Opc_PIO", _Parameters.Item("?Opc_PIO")))
            cmd.Parameters.Add(New MySqlClient.MySqlParameter("?Opc_ZDR", _Parameters.Item("?Opc_ZDR")))
            cmd.Parameters.Add(New MySqlClient.MySqlParameter("?Opc_ZPS", _Parameters.Item("?Opc_ZPS")))

            cmd.Parameters.Add(New MySqlClient.MySqlParameter("?Organizacija", _Parameters.Item("?Organizacija")))

            ConvertStringToDatum(_Parameters.Item("dbDatum"), dbDatum)
            cmd.Parameters.Add(New MySqlClient.MySqlParameter("?Dat_Zaposl", dbDatum))

            ConvertDatumToStaz(dbStaz, _Parameters.Item("dbGod"), _Parameters.Item("dbMje"), _Parameters.Item("dbDani"))
            cmd.Parameters.Add(New MySqlClient.MySqlParameter("?Staz_Prethodni", dbStaz))

            cmd.Parameters.Add(New MySqlClient.MySqlParameter("?RadnoMjesto", _Parameters.Item("?RadnoMjesto")))
            cmd.Parameters.Add(New MySqlClient.MySqlParameter("?Aktivan", _Parameters.Item("?Aktivan")))
            cmd.ExecuteNonQuery()

            ' If _Parameters.Item("?Rukovodilac") Then
            cmd.Parameters.Clear()
            sqlText = "SELECT roles_id FROM my_aspnet_sistematizacija_to_roles WHERE sistematizacija_id=?RadnoMjesto AND roles_id = 1;"
            cmd.CommandText = sqlText
            cmd.Parameters.Add(New MySqlClient.MySqlParameter("?RadnoMjesto", _Parameters.Item("?RadnoMjesto")))
            ldr = cmd.ExecuteReader
            Dim IsRukovodilac As Integer = -1
            While ldr.Read
                Try
                    IsRukovodilac = ldr.Item("roles_id")
                Catch ex As Exception

                End Try
            End While
            ldr.Close()


            If IsRukovodilac <> 1 Then
                Try
                    Roles.RemoveUserFromRole("rlportal" & CStr(_Parameters.Item("?EmployeeID")), "rukovodilac")
                Catch ex As Exception
                End Try
            Else
                Try
                    If Not (Roles.IsUserInRole("rlportal" & CStr(_Parameters.Item("?EmployeeID")), "rukovodilac")) Then
                        Roles.AddUserToRole("rlportal" & CStr(_Parameters.Item("?EmployeeID")), "rukovodilac")
                    End If
                Catch ex As Exception
                End Try
            End If
        Catch ex As Exception

        End Try

        conn.Close()
        conn.Dispose()
    End Sub
    Public Sub Save_EmployeesLPD_ToDB(ByRef _Parameters As Hashtable)
        Dim sqlText As String = "UPDATE evd_employees_lpd SET Invalidnost = ?Invalidnost, Adresa = ?Adresa, adresa_br = ?adresa_br  WHERE EmployeeID = ?EmployeeID;"

        Dim conn As New MySqlClient.MySqlConnection(cnstr)
        Dim cmd As MySqlClient.MySqlCommand
        'Dim ldr As MySql.Data.MySqlClient.MySqlDataReader

        conn.Open()

        'Dim dbDatum As Date
        'Dim dbStaz As Decimal

        Try
            cmd = conn.CreateCommand
            cmd.CommandText = sqlText
            cmd.Parameters.Add(New MySqlClient.MySqlParameter("?EmployeeID", _Parameters.Item("?EmployeeID")))
            cmd.Parameters.Add(New MySqlClient.MySqlParameter("?Invalidnost", _Parameters.Item("?Invalidnost")))
            cmd.Parameters.Add(New MySqlClient.MySqlParameter("?Adresa", _Parameters.Item("?Adresa")))
            cmd.Parameters.Add(New MySqlClient.MySqlParameter("?adresa_br", _Parameters.Item("?adresa_br")))
            cmd.ExecuteNonQuery()



        Catch ex As Exception

        End Try

        conn.Close()
        conn.Dispose()
    End Sub
    Public Sub SaveToDB_Odjava(ByRef _Parameters As Hashtable)
        Dim sqlText As String = "UPDATE evd_employees SET Dat_Odlaska=?Dat_Odlaska, Razlog_Odlaska = ?Razlog_Odlaska, Aktivan = ?Aktivan WHERE EmployeeID = ?EmployeeID;"

        Dim conn As New MySqlClient.MySqlConnection(cnstr)
        Dim cmd As MySqlClient.MySqlCommand
        Dim ldr As MySql.Data.MySqlClient.MySqlDataReader

        conn.Open()

        Dim dbDatum As Date

        Try
            cmd = conn.CreateCommand
            cmd.CommandText = sqlText
            cmd.Parameters.Add(New MySqlClient.MySqlParameter("?EmployeeID", _Parameters.Item("?EmployeeID")))

            ConvertStringToDatum(_Parameters.Item("?Dat_Odlaska"), dbDatum)
            cmd.Parameters.Add(New MySqlClient.MySqlParameter("?Dat_Odlaska", dbDatum))

            cmd.Parameters.Add(New MySqlClient.MySqlParameter("?Razlog_Odlaska", _Parameters.Item("?Razlog_Odlaska")))
            cmd.Parameters.Add(New MySqlClient.MySqlParameter("?Aktivan", _Parameters.Item("?Aktivan")))
            cmd.ExecuteNonQuery()

            ' If _Parameters.Item("?Rukovodilac") Then
            cmd.Parameters.Clear()
            sqlText = "SELECT roles_id FROM my_aspnet_sistematizacija_to_roles WHERE sistematizacija_id=?RadnoMjesto AND roles_id = 1;"
            cmd.CommandText = sqlText
            cmd.Parameters.Add(New MySqlClient.MySqlParameter("?RadnoMjesto", _Parameters.Item("?RadnoMjesto")))
            ldr = cmd.ExecuteReader
            Dim IsRukovodilac As Integer = -1
            While ldr.Read
                Try
                    IsRukovodilac = ldr.Item("roles_id")
                Catch ex As Exception

                End Try
            End While
            ldr.Close()

        Catch ex As Exception

        End Try




        conn.Close()
        conn.Dispose()
    End Sub

    Public Sub GetFromDB(ByRef _Parameters As Hashtable, Optional ByVal WhereValue As String = "EmployeeID")
        Dim lcmd As MySql.Data.MySqlClient.MySqlCommand
        Dim ldr As MySql.Data.MySqlClient.MySqlDataReader
        Dim strSQL As String = ""

        ' Lista ako uposlenik ima rolu Evidencije
        Select Case WhereValue
            Case "EmployeeID"
                strSQL = "SELECT *, DATE(Dat_Zaposl) AS Datum_Zaposl FROM evd_employees WHERE  EmployeeID = " & _Parameters.Item("?EmployeeID") & ";"
            Case "EmployeeJMB"
                strSQL = "SELECT *, DATE(Dat_Zaposl) AS Datum_Zaposl FROM evd_employees WHERE  JMB = '" & _Parameters.Item("?EmployeeJMB") & "';"
        End Select





        Dim conn As New MySqlClient.MySqlConnection(cnstr)
        conn.Open()
        lcmd = New MySql.Data.MySqlClient.MySqlCommand(strSQL, conn)

        Dim fGodina As String = "", fMjesec As String = "", fDani As String = "", fDatum As String = ""

        ldr = lcmd.ExecuteReader
        While ldr.Read
            Try
                Select Case WhereValue
                    Case "EmployeeJMB"
                        _Parameters.Add("?EmployeeID", ldr.Item("EmployeeID"))
                End Select

                _Parameters.Add("?EmployeeNumber", ldr.Item("EmployeeNumber"))
                _Parameters.Add("?JMB", ldr.Item("JMB"))
                _Parameters.Add("?FirstName", ldr.Item("FirstName"))
                _Parameters.Add("?LastName", ldr.Item("LastName"))
                _Parameters.Add("?BrSocOsig", ldr.Item("BrSocOsig"))
                _Parameters.Add("?Opc_Stanovanja", ldr.Item("Opc_Stanovanja"))
                _Parameters.Add("?Opc_Porodice", ldr.Item("Opc_Porodice"))
                _Parameters.Add("?Opc_PIO", ldr.Item("Opc_PIO"))
                _Parameters.Add("?Opc_ZDR", ldr.Item("Opc_ZDR"))
                _Parameters.Add("?Opc_ZPS", ldr.Item("Opc_ZPS"))
                ConvertStazToDatum(ldr.Item("Staz_Prethodni"), fGodina, fMjesec, fDani)

                _Parameters.Add("?Organizacija", ldr.Item("DepartmentUP"))

                _Parameters.Add("fGodina", fGodina)
                _Parameters.Add("fMjesec", fMjesec)
                _Parameters.Add("fDani", fDani)

                ConvertDatumToString(ldr.Item("Datum_Zaposl"), fDatum)
                _Parameters.Add("fDatum", fDatum)

                _Parameters.Add("?RadnoMjesto", ldr.Item("RadnoMjesto"))

            Catch ex As Exception
            End Try
        End While
        ldr.Close()

        Try
            If IsNumeric(_Parameters.Item("?EmployeeID")) Then
                strSQL = "SELECT id FROM sfr_organizacija WHERE Rukovodilac = " & _Parameters.Item("?EmployeeID") & ";"
                lcmd.CommandText = strSQL
                ldr = lcmd.ExecuteReader
                If ldr.HasRows Then
                    _Parameters.Add("?Rukovodilac", True)
                Else
                    _Parameters.Add("?Rukovodilac", False)
                End If

                ldr.Close()
            End If
        Catch ex As Exception

        End Try

    End Sub

    Public Sub Get_EmployeesLPD_FromDB(ByRef _Parameters As Hashtable)
        Dim lcmd As MySql.Data.MySqlClient.MySqlCommand
        Dim ldr As MySql.Data.MySqlClient.MySqlDataReader
        Dim strSQL As String = ""

        ' Lista ako uposlenik ima rolu Evidencije

        strSQL = "SELECT * FROM evd_employees_lpd WHERE  EmployeeID = " & _Parameters.Item("?EmployeeID") & ";"



        Dim conn As New MySqlClient.MySqlConnection(cnstr)
        conn.Open()
        lcmd = New MySql.Data.MySqlClient.MySqlCommand(strSQL, conn)

        Try
            ldr = lcmd.ExecuteReader
            While ldr.Read
                Try
                    _Parameters.Add("?ImeRoditelja", ldr.Item("ImeRoditelja"))
                    _Parameters.Add("?Broj_LK", ldr.Item("Broj_LK"))
                    _Parameters.Add("?Mjesto_LK", ldr.Item("Mjesto_LK"))
                    _Parameters.Add("?Opcina_LK", ldr.Item("Opcina_LK"))
                    _Parameters.Add("?Pol", ldr.Item("Pol"))
                    _Parameters.Add("?Dat_Rodjenja", ldr.Item("Dat_Rodjenja"))
                    _Parameters.Add("?Mjesto_Rodjenja", ldr.Item("Mjesto_Rodjenja"))
                    _Parameters.Add("?Opcina_Rodjenja", ldr.Item("Opcina_Rodjenja"))
                    _Parameters.Add("?Drzava_Rodjenja", ldr.Item("Drzava_Rodjenja"))
                    _Parameters.Add("?Drzavljanstvo", ldr.Item("Drzavljanstvo"))
                    _Parameters.Add("?Narod", ldr.Item("Narod"))
                    _Parameters.Add("?BracnoStanje", ldr.Item("BracnoStanje"))
                    _Parameters.Add("?Supruznik", ldr.Item("Supruznik"))
                    _Parameters.Add("?Djeca", ldr.Item("Djeca"))
                    _Parameters.Add("?KrvnaGrupa", ldr.Item("KrvnaGrupa"))
                    _Parameters.Add("?KontaktOsoba", ldr.Item("KontaktOsoba"))
                    _Parameters.Add("?Pripravnik", ldr.Item("Pripravnik"))
                    _Parameters.Add("?StatusZaposlenja", ldr.Item("StatusZaposlenja"))
                    _Parameters.Add("?DjecijiDodatak", ldr.Item("DjecijiDodatak"))
                    _Parameters.Add("?TerenskiDodatak", ldr.Item("TerenskiDodatak"))
                    _Parameters.Add("?BrojClanovaDomacinstva", ldr.Item("BrojClanovaDomacinstva"))
                    _Parameters.Add("?StrucnaSprema", ldr.Item("StrucnaSprema"))
                    _Parameters.Add("?NazivZavrseneSkole", ldr.Item("NazivZavrseneSkole"))
                    _Parameters.Add("?NazivTekuceSkole", ldr.Item("NazivTekuceSkole"))
                    _Parameters.Add("?Zanimanje", ldr.Item("Zanimanje"))
                    _Parameters.Add("?StrucniIspit", ldr.Item("StrucniIspit"))
                    _Parameters.Add("?SpecijalistickaObuka", ldr.Item("SpecijalistickaObuka"))
                    _Parameters.Add("?PosebnaZnanja", ldr.Item("PosebnaZnanja"))
                    _Parameters.Add("?PoznavanjeRacunara", ldr.Item("PoznavanjeRacunara"))
                    _Parameters.Add("?OcjenaRada", ldr.Item("OcjenaRada"))
                    _Parameters.Add("?Invalidnost", ldr.Item("Invalidnost"))
                    _Parameters.Add("?PovredaRadneDiscipline", ldr.Item("PovredaRadneDiscipline"))
                    _Parameters.Add("?SamohraniRoditelj", ldr.Item("SamohraniRoditelj"))
                    _Parameters.Add("?Adresa", ldr.Item("Adresa"))
                    _Parameters.Add("?Tel_pos", ldr.Item("Tel_pos"))
                    _Parameters.Add("?Tel_prv", ldr.Item("Tel_prv"))
                    _Parameters.Add("?Mob_prv", ldr.Item("Mob_prv"))
                    _Parameters.Add("?Email_pos", ldr.Item("Email_pos"))
                    _Parameters.Add("?adresa_br", ldr.Item("adresa_br"))



                Catch ex As Exception
                End Try
            End While
            ldr.Close()
        Catch ex As Exception

        End Try





    End Sub

    Private Sub ConvertStazToDatum(ByVal dbStaz As Decimal, ByRef fGodina As String, ByRef fMjesec As String, ByRef fDani As String)

        fGodina = (Decimal.Truncate(Decimal.Truncate(dbStaz) / 12)).ToString
        fMjesec = (Decimal.Remainder(Decimal.Truncate(dbStaz), 12)).ToString
        fDani = (Decimal.Truncate((dbStaz - Decimal.Truncate(dbStaz)) * 100)).ToString

    End Sub

    Private Sub ConvertDatumToStaz(ByRef dbStaz As Decimal, ByVal fGodina As String, ByVal fMjesec As String, ByVal fDani As String)
        Dim decStaz1, decStaz2 As Decimal

        Decimal.TryParse(fGodina, decStaz1)
        decStaz1 = decStaz1 * 12
        Decimal.TryParse(fMjesec, decStaz2)
        decStaz1 = decStaz1 + decStaz2

        Decimal.TryParse(fDani, decStaz2)
        dbStaz = decStaz1 + decStaz2 / 100


    End Sub

    Private Sub ConvertDatumToString(ByVal dbDatum As MySql.Data.Types.MySqlDateTime, ByRef fDatum As String)

        fDatum = (dbDatum.Day).ToString & "-" & (dbDatum.Month).ToString & "-" & (dbDatum.Year).ToString

    End Sub

    Private Sub ConvertStringToDatum(ByVal fDatum As String, ByRef dbDatum As Date)
        Dim tArr() As String = fDatum.Split("-")
        Dim tDatum As String = ""

        Try
            tDatum = tArr(2) & "/" & tArr(1) & "/" & tArr(0)
        Catch ex As Exception

        End Try

        dbDatum = Date.Now
        Try
            dbDatum = Date.Parse(tDatum)
        Catch ex As Exception

        End Try

    End Sub

    Public Sub CreateUsersAtStartUp()
        Dim lcmd As MySql.Data.MySqlClient.MySqlCommand
        Dim ldr As MySql.Data.MySqlClient.MySqlDataReader
        Dim strSQL As String

        strSQL = "SELECT * FROM evd_employees WHERE EmployeeID <>55;"

        Dim conn As New MySqlClient.MySqlConnection(cnstr)
        conn.Open()
        lcmd = New MySql.Data.MySqlClient.MySqlCommand(strSQL, conn)

        ldr = lcmd.ExecuteReader
        While ldr.Read
            Try
                AddUserToRoles(ldr.Item("EmployeeID"))
            Catch ex As Exception
            End Try
        End While
        ldr.Close()
    End Sub

    Private Sub AddUserToRoles(ByVal newEmployeeID As Integer)
        If newEmployeeID <> -1 Then
            Dim newUserName As String = "rlportal" & newEmployeeID.ToString
            Membership.CreateUser(newUserName, newUserName, "rl@rl.ba")
            Dim newUserId As Integer = Get_INSERT_USERID_FromDB(newUserName)
            If newUserId > -1 Then
                Roles.AddUserToRole(newUserName, "uposlenik")
                Insert_UserToEmployee_ToDB(newUserId, newEmployeeID)
            Else
                Exit Sub
            End If
        Else
            Exit Sub
        End If
    End Sub

    Public Shared Sub GetEmployees_List(ByRef _ListBox As System.Web.UI.WebControls.ListBox)
        Dim ConnectionString As String = ConfigurationManager.ConnectionStrings("Evidencije").ConnectionString
        Dim con As New MySqlClient.MySqlConnection(ConnectionString)

        Dim SQL As String = " SELECT t3.EmployeeID, t3.LastName, t3.FirstName FROM my_aspnet_users t1 INNER JOIN my_aspnet_users_to_employees t2 " _
                & "ON (t1.id = t2.users_id) INNER JOIN evd_employees t3 ON (t2.employees_id = t3.EmployeeID) " _
                & "WHERE t2.employees_id = t2.employees_id   AND t1.id IN (SELECT userId  FROM my_aspnet_membership WHERE IsApproved = TRUE) AND t3.Aktivan<>0 Order By t3.LastName Asc;"

        Dim cmd As New MySqlClient.MySqlCommand(SQL, con)

        _ListBox.Items.Clear()
        Try
            con.Open()
            cmd.ExecuteNonQuery()
            Dim rd As MySqlClient.MySqlDataReader = cmd.ExecuteReader()
            Do While rd.Read
                _ListBox.Items.Add(New System.Web.UI.WebControls.ListItem(rd("LastName") & " " & rd("FirstName"), rd("EmployeeID")))
            Loop
            rd.Close()
            con.Close()
        Catch ex As Exception

        End Try
    End Sub

    Public Shared Sub GetEmployees_ListNeodobrene(ByRef _ListBox As System.Web.UI.WebControls.ListBox, _
                                                  ByVal curYear As Integer, ByVal curMonth As Integer)
        Dim ConnectionString As String = ConfigurationManager.ConnectionStrings("Evidencije").ConnectionString
        Dim con As New MySqlClient.MySqlConnection(ConnectionString)

        Dim SQL As String = "SELECT t3.EmployeeID, t3.LastName, t3.FirstName FROM my_aspnet_users t1 INNER JOIN my_aspnet_users_to_employees t2 " _
                        & " ON (t1.id = t2.users_id) INNER JOIN evd_employees t3 ON (t2.employees_id = t3.EmployeeID) " _
                        & "WHERE t2.employees_id = t2.employees_id   AND t1.id IN (SELECT userId  FROM my_aspnet_membership WHERE IsApproved = TRUE) AND t3.Aktivan<>0 " _
                        & "AND t3.EmployeeID IN (SELECT DISTINCT employeeID FROM plc_portal.evd_prisustva " _
                        & " WHERE(evd_podnio Is NULL Or evd_odobrio Is NULL Or evd_kontrolisao Is NULL) " _
                        & " AND YEAR(datum)=" & curYear & " AND MONTH(datum)=" & curMonth & ") ORDER BY t3.LastName ASC;"

        Dim cmd As New MySqlClient.MySqlCommand(SQL, con)

        _ListBox.Items.Clear()
        Try
            con.Open()
            cmd.ExecuteNonQuery()
            Dim rd As MySqlClient.MySqlDataReader = cmd.ExecuteReader()
            Do While rd.Read
                _ListBox.Items.Add(New System.Web.UI.WebControls.ListItem(rd("LastName") & " " & rd("FirstName"), rd("EmployeeID")))
            Loop
            rd.Close()
            con.Close()
        Catch ex As Exception

        End Try
    End Sub

    Public Shared Sub GetEmployees_ListNeodobrene(ByRef _Label As System.Web.UI.WebControls.Label, _
                                              ByVal curYear As Integer, ByVal curMonth As Integer)
        Dim ConnectionString As String = ConfigurationManager.ConnectionStrings("Evidencije").ConnectionString
        Dim con As New MySqlClient.MySqlConnection(ConnectionString)

        Dim SQL As String = "SELECT t3.EmployeeID, t3.LastName, t3.FirstName FROM my_aspnet_users t1 INNER JOIN my_aspnet_users_to_employees t2 " _
                        & " ON (t1.id = t2.users_id) INNER JOIN evd_employees t3 ON (t2.employees_id = t3.EmployeeID) " _
                        & "WHERE t2.employees_id = t2.employees_id   AND t1.id IN (SELECT userId  FROM my_aspnet_membership WHERE IsApproved = TRUE) AND t3.Aktivan<>0 " _
                        & "AND t3.EmployeeID IN (SELECT DISTINCT employeeID FROM plc_portal.evd_prisustva " _
                        & " WHERE(evd_podnio Is NULL Or evd_odobrio Is NULL Or evd_kontrolisao Is NULL) " _
                        & " AND YEAR(datum)=" & curYear & " AND MONTH(datum)=" & curMonth & ") ORDER BY t3.LastName ASC;"

        Dim cmd As New MySqlClient.MySqlCommand(SQL, con)
        _Label.Text = "Evidencije nisu odobrene niti kontrolisane za sljedeće uposlenike: "
        Try
            con.Open()
            cmd.ExecuteNonQuery()
            Dim rd As MySqlClient.MySqlDataReader = cmd.ExecuteReader()

            If rd.HasRows Then
                _Label.Visible = True
            Else
                _Label.Visible = False
            End If

            Do While rd.Read
                _Label.Text = _Label.Text & rd("LastName") & " " & rd("FirstName") & ", "
            Loop
            rd.Close()
            con.Close()
        Catch ex As Exception

        End Try
    End Sub

    Public Shared Sub GetEmployees_BezLista(ByRef _Label As System.Web.UI.WebControls.Label, _
                                          ByVal curYear As Integer, ByVal curMonth As Integer)
        Dim ConnectionString As String = ConfigurationManager.ConnectionStrings("Evidencije").ConnectionString
        Dim con As New MySqlClient.MySqlConnection(ConnectionString)

        Dim SQL As String = "SELECT t3.EmployeeID, t3.LastName, t3.FirstName FROM my_aspnet_users t1 INNER JOIN my_aspnet_users_to_employees t2 " _
                        & " ON (t1.id = t2.users_id) INNER JOIN evd_employees t3 ON (t2.employees_id = t3.EmployeeID) " _
                        & "WHERE t2.employees_id = t2.employees_id   AND t1.id IN (SELECT userId  FROM my_aspnet_membership WHERE IsApproved = TRUE) AND t3.Aktivan<>0 " _
                        & "AND t3.EmployeeID NOT IN (SELECT DISTINCT employeeID FROM plc_portal.evd_prisustva " _
                        & " ) ORDER BY t3.LastName ASC;"

        Dim cmd As New MySqlClient.MySqlCommand(SQL, con)
        _Label.Text = "Nema evidencije za sljedeće uposlenike: "
        Try
            con.Open()
            cmd.ExecuteNonQuery()
            Dim rd As MySqlClient.MySqlDataReader = cmd.ExecuteReader()

            If rd.HasRows Then
                _Label.Visible = True
            Else
                _Label.Visible = False
            End If

            Do While rd.Read
                _Label.Text = _Label.Text & rd("LastName") & " " & rd("FirstName") & ", "
            Loop
            rd.Close()
            con.Close()
        Catch ex As Exception

        End Try
    End Sub


    Public Shared Sub GetEmployees_BezLista(ByRef _ListBox As System.Web.UI.WebControls.ListBox, _
                                                  ByVal curYear As Integer, ByVal curMonth As Integer)
        Dim ConnectionString As String = ConfigurationManager.ConnectionStrings("Evidencije").ConnectionString
        Dim con As New MySqlClient.MySqlConnection(ConnectionString)

        Dim SQL As String = "SELECT t3.EmployeeID, t3.LastName, t3.FirstName FROM my_aspnet_users t1 INNER JOIN my_aspnet_users_to_employees t2 " _
                        & " ON (t1.id = t2.users_id) INNER JOIN evd_employees t3 ON (t2.employees_id = t3.EmployeeID) " _
                        & "WHERE t2.employees_id = t2.employees_id   AND t1.id IN (SELECT userId  FROM my_aspnet_membership WHERE IsApproved = TRUE) AND t3.Aktivan<>0 " _
                        & "AND t3.EmployeeID NOT IN (SELECT DISTINCT employeeID FROM plc_portal.evd_prisustva " _
                        & " ) ORDER BY t3.LastName ASC;"

        Dim cmd As New MySqlClient.MySqlCommand(SQL, con)

        _ListBox.Items.Clear()
        Try
            con.Open()
            cmd.ExecuteNonQuery()
            Dim rd As MySqlClient.MySqlDataReader = cmd.ExecuteReader()
            Do While rd.Read
                _ListBox.Items.Add(New System.Web.UI.WebControls.ListItem(rd("LastName") & " " & rd("FirstName"), rd("EmployeeID")))
            Loop
            rd.Close()
            con.Close()
        Catch ex As Exception

        End Try
    End Sub

    Public Shared Sub GetEmployees_BezPunihSati(ByRef _ListBox As System.Web.UI.WebControls.ListBox, _
                                              ByVal curYear As Integer, ByVal curMonth As Integer)
        Dim ConnectionString As String = ConfigurationManager.ConnectionStrings("Evidencije").ConnectionString
        Dim con As New MySqlClient.MySqlConnection(ConnectionString)

        Dim dt As Date = New Date(curYear, curMonth, 1)
        Dim totalSati As Integer = 0
        For i As Integer = 1 To Date.DaysInMonth(curYear, curMonth)

            If dt.DayOfWeek <> DayOfWeek.Sunday And dt.DayOfWeek <> DayOfWeek.Saturday Then
                totalSati = totalSati + 1
            End If
            dt = dt.AddDays(1)
        Next

        totalSati = totalSati * 8

        Dim SQL As String = "SELECT t3.EmployeeID, t3.LastName, t3.FirstName FROM my_aspnet_users t1 INNER JOIN my_aspnet_users_to_employees t2 " _
                        & " ON (t1.id = t2.users_id) INNER JOIN evd_employees t3 ON (t2.employees_id = t3.EmployeeID) " _
                        & "WHERE t2.employees_id = t2.employees_id   AND t1.id IN (SELECT userId  FROM my_aspnet_membership WHERE IsApproved = TRUE) AND t3.Aktivan<>0 " _
                        & "AND t3.EmployeeID IN (SELECT  employeeID FROM evd_prisustva_suma WHERE godina = " & curYear.ToString & " AND mjesec=" & curMonth.ToString & " " _
                        & " GROUP BY employeeID, godina, mjesec HAVING SUM(suma_sati)<" & totalSati.ToString & " " _
                        & " ) ORDER BY t3.LastName ASC;"

        Dim cmd As New MySqlClient.MySqlCommand(SQL, con)

        _ListBox.Items.Clear()
        Try
            con.Open()
            cmd.ExecuteNonQuery()
            Dim rd As MySqlClient.MySqlDataReader = cmd.ExecuteReader()
            Do While rd.Read
                _ListBox.Items.Add(New System.Web.UI.WebControls.ListItem(rd("LastName") & " " & rd("FirstName"), rd("EmployeeID")))
            Loop
            rd.Close()
            con.Close()
        Catch ex As Exception

        End Try
    End Sub

    Public Shared Sub GetEmployees_BezPunihSati(ByRef _Label As System.Web.UI.WebControls.Label, _
                                      ByVal curYear As Integer, ByVal curMonth As Integer)
        Dim ConnectionString As String = ConfigurationManager.ConnectionStrings("Evidencije").ConnectionString
        Dim con As New MySqlClient.MySqlConnection(ConnectionString)

        Dim dt As Date = New Date(curYear, curMonth, 1)
        Dim totalSati As Integer = 0
        For i As Integer = 1 To Date.DaysInMonth(curYear, curMonth)

            If dt.DayOfWeek <> DayOfWeek.Sunday And dt.DayOfWeek <> DayOfWeek.Saturday Then
                totalSati = totalSati + 1
            End If
            dt = dt.AddDays(1)
        Next

        totalSati = totalSati * 8

        Dim SQL As String = "SELECT t3.EmployeeID, t3.LastName, t3.FirstName FROM my_aspnet_users t1 INNER JOIN my_aspnet_users_to_employees t2 " _
                        & " ON (t1.id = t2.users_id) INNER JOIN evd_employees t3 ON (t2.employees_id = t3.EmployeeID) " _
                        & "WHERE t2.employees_id = t2.employees_id   AND t1.id IN (SELECT userId  FROM my_aspnet_membership WHERE IsApproved = TRUE) AND t3.Aktivan<>0 " _
                        & "AND t3.EmployeeID IN (SELECT  employeeID FROM evd_prisustva_suma WHERE godina = " & curYear.ToString & " AND mjesec=" & curMonth.ToString & " " _
                        & " GROUP BY employeeID, godina, mjesec HAVING SUM(suma_sati)<" & totalSati.ToString & " " _
                        & " ) ORDER BY t3.LastName ASC;"

        Dim cmd As New MySqlClient.MySqlCommand(SQL, con)
        _Label.Text = "Sljedeći uposlenici su bez punih sati: "
        Try
            con.Open()
            cmd.ExecuteNonQuery()
            Dim rd As MySqlClient.MySqlDataReader = cmd.ExecuteReader()

            If rd.HasRows Then
                _Label.Visible = True
            Else
                _Label.Visible = False
            End If

            Do While rd.Read
                _Label.Text = _Label.Text & rd("LastName") & " " & rd("FirstName") & ", "
            Loop
            rd.Close()
            con.Close()
        Catch ex As Exception

        End Try
    End Sub

    Public Shared Function GetEmployees_SviDepartmentUP(ByVal EmployeeID As Integer, ByRef clsMySqlConnection As MySql.Data.MySqlClient.MySqlConnection) As String

        Dim DepartmentUPLista_Tmp As New System.Collections.Generic.List(Of Integer)
        Dim lcmd As MySql.Data.MySqlClient.MySqlCommand
        Dim ldr As MySql.Data.MySqlClient.MySqlDataReader
        Dim strSQL As String
        Dim DepartmentUP, RadnoMjesto As Integer
        Dim DepartmentUPLista_String As String = ""

        strSQL = "SELECT DepartmentUP, RadnoMjesto FROM evd_employees WHERE employeeid =" & EmployeeID & ";"
        lcmd = New MySql.Data.MySqlClient.MySqlCommand(strSQL, clsMySqlConnection)

        ldr = lcmd.ExecuteReader
        While ldr.Read
            Try
                DepartmentUP = ldr.Item("DepartmentUP")
                RadnoMjesto = ldr.Item("RadnoMjesto")
            Catch ex As Exception
            End Try
        End While
        ldr.Close()

        ClsUposleniPodaci.GetOrganizacija_SubLevels(DepartmentUP, DepartmentUPLista_Tmp, clsMySqlConnection)
        ClsUposleniPodaci.GetOrganizacija_Externals(RadnoMjesto, DepartmentUPLista_Tmp, clsMySqlConnection)


        DepartmentUPLista_String = TransformListToCSV(DepartmentUPLista_Tmp)
        Return DepartmentUPLista_String

    End Function

    Public Shared Sub GetOrganizacija_SubLevels(ByVal DepartmentID As Integer, ByRef DepartmentIDLista As System.Collections.Generic.List(Of Integer), _
                               ByRef clsMySqlConnection As MySql.Data.MySqlClient.MySqlConnection)

        Dim lcmd As MySql.Data.MySqlClient.MySqlCommand
        Dim ldr As MySql.Data.MySqlClient.MySqlDataReader
        Dim strSQL As String

        strSQL = "SELECT t1.id AS lev1, t2.id AS lev2, t3.id AS lev3, t4.id AS lev4, t5.id AS lev5" _
                & " FROM sfr_organizacija AS t1 " _
                & " LEFT JOIN sfr_organizacija AS t2 ON t2.`Sifra Nadnivo` = t1.Sifra " _
                & " LEFT JOIN sfr_organizacija AS t3 ON t3.`Sifra Nadnivo` = t2.Sifra " _
                & " LEFT JOIN sfr_organizacija AS t4 ON t4.`Sifra Nadnivo` = t3.Sifra " _
                & " LEFT JOIN sfr_organizacija AS t5 ON t5.`Sifra Nadnivo` = t4.Sifra " _
                & " WHERE t1.id = " & DepartmentID & ";"

        lcmd = New MySql.Data.MySqlClient.MySqlCommand(strSQL, clsMySqlConnection)

        ldr = lcmd.ExecuteReader
        While ldr.Read
            Try
                If Not DepartmentIDLista.Contains(ldr.Item("lev1")) Then
                    DepartmentIDLista.Add(ldr.Item("lev1"))
                End If
                If Not DepartmentIDLista.Contains(ldr.Item("lev2")) Then
                    DepartmentIDLista.Add(ldr.Item("lev2"))
                End If
                If Not DepartmentIDLista.Contains(ldr.Item("lev3")) Then
                    DepartmentIDLista.Add(ldr.Item("lev3"))
                End If
                If Not DepartmentIDLista.Contains(ldr.Item("lev4")) Then
                    DepartmentIDLista.Add(ldr.Item("lev4"))
                End If
                If Not DepartmentIDLista.Contains(ldr.Item("lev5")) Then
                    DepartmentIDLista.Add(ldr.Item("lev5"))
                End If
            Catch ex As Exception
            End Try
        End While
        ldr.Close()

    End Sub

    Public Shared Sub GetOrganizacija_Externals(ByVal RadnoMjestoID As Integer, ByRef DepartmentIDLista As System.Collections.Generic.List(Of Integer), _
                           ByRef clsMySqlConnection As MySql.Data.MySqlClient.MySqlConnection)
        ' Za radnomjesto koje ima rolu rukovodilac uzima listu eksternih organizacionih djelova
        Dim lcmd As MySql.Data.MySqlClient.MySqlCommand
        Dim ldr As MySql.Data.MySqlClient.MySqlDataReader
        Dim strSQL As String

        strSQL = "SELECT s2o.organizacija_id FROM " _
                & " plc_portal.sfr_sistematizacija AS s " _
                & " INNER JOIN plc_portal.my_aspnet_sistematizacija_to_roles AS s2r " _
                & " ON (s.id = s2r.sistematizacija_id) " _
                & " INNER JOIN plc_portal.sfr_sistematizacija_to_organizacija AS s2o " _
                & " ON (s2r.sistematizacija_id = s2o.sistematizacija_id) WHERE s2r.roles_id=1 AND s.id=" & RadnoMjestoID & ";"

        lcmd = New MySql.Data.MySqlClient.MySqlCommand(strSQL, clsMySqlConnection)

        ldr = lcmd.ExecuteReader
        While ldr.Read
            Try
                If Not DepartmentIDLista.Contains(ldr.Item("organizacija_id")) Then
                    DepartmentIDLista.Add(ldr.Item("organizacija_id"))
                End If
            Catch ex As Exception
            End Try
        End While
        ldr.Close()

    End Sub

    Public Shared Function TransformListToCSV(ByRef DepartmentUPLista_Tmp As System.Collections.Generic.List(Of Integer)) As String
        Dim DepartmentUPLista_String As String = ""

        Try
            For i As Integer = 0 To DepartmentUPLista_Tmp.Count - 1
                ' Iskljuci upravu iz liste ??????????????
                If i < DepartmentUPLista_Tmp.Count - 1 Then
                    DepartmentUPLista_String &= DepartmentUPLista_Tmp.Item(i).ToString & ","
                Else
                    DepartmentUPLista_String &= DepartmentUPLista_Tmp.Item(i).ToString
                End If
            Next
        Catch ex As Exception

        End Try


        Return DepartmentUPLista_String
    End Function
End Class
