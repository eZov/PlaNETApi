Imports Microsoft.VisualBasic
Imports System.Web.Configuration
Imports System.Data.SqlClient
Imports System.Data
Imports MySql.Data
Imports System.Math
Imports System.Web.UI.WebControls
Imports System
Imports System.Xml.Linq

Public Class ClsPutniNalozi
    Public Shared Function Uposlenici_ListaByRole(ByVal RoleName As String, Optional ByVal EmployeeID As Integer = -1) As String

        Dim lcmd As MySql.Data.MySqlClient.MySqlCommand
        Dim ldr As MySql.Data.MySqlClient.MySqlDataReader
        Dim strSQL As String
        Dim listaEmployeeID As String = ""
        Dim clsMySqlConnection As New MySql.Data.MySqlClient.MySqlConnection(ConnectionString)

        clsMySqlConnection.Open()

        Try
            Select Case RoleName

                Case "pravna"
                    strSQL = "SELECT 	EmployeeID, Concat(LastName,' ',FirstName) As Name, DepartmentUP FROM evd_employees WHERE  Aktivan <>0 Order By LastName;"
                    lcmd = New MySql.Data.MySqlClient.MySqlCommand(strSQL, clsMySqlConnection)

                    Try
                        ldr = lcmd.ExecuteReader
                        While ldr.Read
                            Try
                                listaEmployeeID += "," + ldr.Item("EmployeeID").ToString
                            Catch ex As Exception
                            End Try
                        End While
                        ldr.Close()
                    Catch ex As Exception
                    End Try



                Case "evidencija"
                    strSQL = "SELECT EmployeeID, Concat(LastName,' ',FirstName) As Name, DepartmentUP FROM evd_employees WHERE  Aktivan <>0 Order By LastName;"
                    lcmd = New MySql.Data.MySqlClient.MySqlCommand(strSQL, clsMySqlConnection)

                    Try
                        ldr = lcmd.ExecuteReader
                        While ldr.Read
                            Try
                                listaEmployeeID += "," + ldr.Item("EmployeeID").ToString
                            Catch ex As Exception
                            End Try
                        End While
                        ldr.Close()
                    Catch ex As Exception
                    End Try


                Case "rukovodilac"
                    strSQL = "SELECT EmployeeID, Concat(LastName,' ',FirstName) As Name, DepartmentUP 	FROM evd_employees WHERE DepartmentUP IN(" & ClsUposleniPodaci.GetEmployees_SviDepartmentUP(EmployeeID, clsMySqlConnection) & ") AND EmployeeID NOT IN(" & EmployeeID & ") AND Aktivan <>0 Order By LastName;"
                    lcmd = New MySql.Data.MySqlClient.MySqlCommand(strSQL, clsMySqlConnection)

                    Try
                        ldr = lcmd.ExecuteReader
                        While ldr.Read
                            Try
                                listaEmployeeID += "," + ldr.Item("EmployeeID").ToString
                            Catch ex As Exception
                            End Try
                        End While
                        ldr.Close()
                    Catch ex As Exception
                    End Try

                Case "uprava"
                    strSQL = "SELECT EmployeeID, Concat(LastName,' ',FirstName) As Name, DepartmentUP 	FROM evd_employees WHERE (DepartmentUp = 5 OR EmployeeID IN (SELECT e.EmployeeID FROM my_aspnet_membership AS u INNER JOIN my_aspnet_users_to_employees AS u2e " _
                    & " ON (u.userId = u2e.users_id) INNER JOIN evd_employees AS e ON (e.EmployeeID = u2e.employees_id) " _
                    & " INNER JOIN my_aspnet_sistematizacija_to_roles AS s2r ON (e.RadnoMjesto = s2r.sistematizacija_id) WHERE s2r.roles_id=1)) AND Aktivan <>0 Order By LastName;"
                    lcmd = New MySql.Data.MySqlClient.MySqlCommand(strSQL, clsMySqlConnection)

                    Try
                        ldr = lcmd.ExecuteReader
                        While ldr.Read
                            Try
                                listaEmployeeID += "," + ldr.Item("EmployeeID").ToString
                            Catch ex As Exception
                            End Try
                        End While
                        ldr.Close()
                    Catch ex As Exception
                    End Try

                Case Else
                    listaEmployeeID = "," + EmployeeID.ToString
            End Select

            If listaEmployeeID.Length > 1 Then
                listaEmployeeID = listaEmployeeID.Substring(1)
            End If
        Catch ex As Exception

        End Try



        clsMySqlConnection.Close()
        Return listaEmployeeID

    End Function


    Public Shared Sub getPutniNalozi(ByVal employee_id As Integer, ByVal status_naloga As Integer, _
                    ByRef ddlPutniNalog As System.Web.UI.WebControls.DropDownList)

        ' Dim ConnectionString As String = ApiGlobal.domainConnectionString

        Dim con As New MySqlClient.MySqlConnection(ConnectionString)
        Dim SQL As String = "SELECT PutniNalog.putninalog_id, PutniNalog.mjesto_putovanja , PutniNalog.date_added  FROM PutniNalog " _
                          & "WHERE PutniNalog.employee_id=" & employee_id & " AND PutniNalog.status_naloga=" & status_naloga & " Order By date_added Desc;"

        Dim cmd As New MySqlClient.MySqlCommand(SQL, con)
        Dim tmpMySqlDate As MySql.Data.Types.MySqlDateTime

        con.Open()
        cmd.ExecuteNonQuery()
        Dim rd As MySqlClient.MySqlDataReader = cmd.ExecuteReader()
        Do While rd.Read
            tmpMySqlDate = rd("date_added")

            ddlPutniNalog.Items.Add(New System.Web.UI.WebControls.ListItem(rd("mjesto_putovanja"), rd("putninalog_id")))
        Loop
        rd.Close()
        con.Close()

    End Sub

    Public Shared Function getPutniNalogStatus(ByVal putninalog_id As Integer) As String

        ' Dim ConnectionString As String = ApiGlobal.domainConnectionString

        Dim con As New MySqlClient.MySqlConnection(ConnectionString)
        Dim SQL As String = "SELECT putninalog_status.status_text FROM putninalog INNER JOIN plc_portal.putninalog_status " _
                        & "ON (putninalog.status_naloga = putninalog_status.status_id) WHERE putninalog.putninalog_id = " & putninalog_id & ";"

        Dim cmd As New MySqlClient.MySqlCommand(SQL, con)
        Dim putninalog As String = ""

        con.Open()
        cmd.ExecuteNonQuery()
        Dim rd As MySqlClient.MySqlDataReader = cmd.ExecuteReader()
        Do While rd.Read
            putninalog = rd("status_text")
        Loop
        rd.Close()
        con.Close()

        Return putninalog
    End Function

    Public Shared Sub CreatePutniNalog(ByVal employee_id As Integer, ByVal EmployeeName As String, ByRef putninalog_id As Integer)
        Dim con As New MySqlClient.MySqlConnection(ConnectionString)
        Dim tmpMySqlDate As String = "'" & Now().Year & "/" & Now().Month & "/" & Now().Day & "'"

        Dim sqlText As String = "SELECT Naziv AS RadnoMjesto FROM  evd_employees e INNER JOIN sfr_sistematizacija s ON (e.RadnoMjesto = s.id) WHERE EmployeeID = " & employee_id & ";"
        Dim cmd As New MySqlClient.MySqlCommand(sqlText, con)


        Try

            con.Open()

            Dim RadnoMjesto As String = ""
            Dim rd As MySqlClient.MySqlDataReader = cmd.ExecuteReader()
            If rd.Read Then RadnoMjesto = CheckDBNull(rd("RadnoMjesto"))

            rd.Close()


            sqlText = "INSERT INTO PutniNalog(employee_id,date_added, dat_poc_putovanja, status_naloga,mjesto_putovanja, ime_prezime, radno_mjesto ) " _
            & "VALUES(" & employee_id & "," & tmpMySqlDate & "," & tmpMySqlDate & " , 1, 'Novi nalog', '" & EmployeeName & "', '" & RadnoMjesto & "');"
            cmd.CommandText = sqlText


            cmd.ExecuteNonQuery()

            sqlText = "SELECT putninalog_id FROM PutniNalog WHERE employee_id=" & employee_id & " AND mjesto_putovanja='Novi nalog' AND status_naloga = 1 Order By putninalog_id DESC;"

            cmd.CommandText = sqlText


            rd = cmd.ExecuteReader()
            If rd.Read Then putninalog_id = CheckDBNull(rd("putninalog_id"))

            rd.Close()
            con.Close()
            CreateObracunNaloga(putninalog_id)

        Catch ex As Exception

        End Try



    End Sub

    Public Shared Sub CreateObracunNaloga(ByVal putninalog_id As Integer)

        Dim con As New MySqlClient.MySqlConnection(ConnectionString)
        Dim strSQL As String
        Dim tmpMySqlDate As String = "'" & Now().Year & "/" & Now().Month & "/" & Now().Day & "'"
        Dim cmd As New MySqlClient.MySqlCommand()
        cmd.Connection = con
        con.Open()


        strSQL = "INSERT INTO PutniNalog_Obracun(putninalog_id, putninalog_oblast,order_id, dat_polaska) VALUES(" & putninalog_id & ", 1, 1, " & tmpMySqlDate & ");"
        cmd.CommandText = strSQL
        cmd.ExecuteNonQuery()
        ' Nema unosa početka putovanja 
        ' strSQL = "INSERT INTO PutniNalog_Obracun(putninalog_id, putninalog_oblast,order_id, dat_polaska) VALUES(" & putninalog_id & ", 1, 2, " & tmpMySqlDate & ");"
        strSQL = "INSERT INTO PutniNalog_Obracun(putninalog_id, putninalog_oblast,order_id) VALUES(" & putninalog_id & ", 1, 2 );"
        cmd.CommandText = strSQL
        cmd.ExecuteNonQuery()
        ' strSQL = "INSERT INTO PutniNalog_Obracun(putninalog_id, putninalog_oblast,order_id, dat_polaska) VALUES(" & putninalog_id & ", 1, 3, " & tmpMySqlDate & ");"
        strSQL = "INSERT INTO PutniNalog_Obracun(putninalog_id, putninalog_oblast,order_id) VALUES(" & putninalog_id & ", 1, 3 );"
        cmd.CommandText = strSQL
        cmd.ExecuteNonQuery()
        strSQL = "INSERT INTO PutniNalog_Obracun(putninalog_id, putninalog_oblast,order_id) VALUES(" & putninalog_id & ", 2, 1);"
        cmd.CommandText = strSQL
        cmd.ExecuteNonQuery()
        strSQL = "INSERT INTO PutniNalog_Obracun(putninalog_id, putninalog_oblast,order_id) VALUES(" & putninalog_id & ", 2, 2);"
        cmd.CommandText = strSQL
        cmd.ExecuteNonQuery()
        strSQL = "INSERT INTO PutniNalog_Obracun(putninalog_id, putninalog_oblast,order_id) VALUES(" & putninalog_id & ", 2, 3);"
        cmd.CommandText = strSQL
        cmd.ExecuteNonQuery()
        strSQL = "INSERT INTO PutniNalog_Obracun(putninalog_id, putninalog_oblast,order_id) VALUES(" & putninalog_id & ", 3, 1);"
        cmd.CommandText = strSQL
        cmd.ExecuteNonQuery()
        strSQL = "INSERT INTO PutniNalog_Obracun(putninalog_id, putninalog_oblast,order_id) VALUES(" & putninalog_id & ", 3, 2);"
        cmd.CommandText = strSQL
        cmd.ExecuteNonQuery()
        strSQL = "INSERT INTO PutniNalog_Obracun(putninalog_id, putninalog_oblast,order_id) VALUES(" & putninalog_id & ", 3, 3);"
        cmd.CommandText = strSQL
        cmd.ExecuteNonQuery()
        con.Close()

    End Sub

    Public Shared Function savePutniNalog(ByRef pPutniNalog As ClsPutniNalog) As Boolean


        Dim retVal As Boolean = False
        Dim strSQL As String
        Dim xmlSQL As XElement

        Select Case pPutniNalog.putninalog_id
            Case -1
                pPutniNalog.status_naloga = 1

                xmlSQL = <SQL>
                        INSERT INTO `putninalog` (
                          `employee_id`,
                          `radno_mjesto`,
                          `mjesto_putovanja`,
                          `razlog_putovanja`,
                          `dat_poc_putovanja`,
                          `trajanje_putovanja`,
                          `prev_auto`,
                          `prev_vlauto`,
                          `prev_avion`,
                          `prev_autobus`,
                          `prevoz_voz`,
                          `iznos_akontacije`,
                          `status_naloga`,
                          `troskovi_putovanja_knjizenje`,
                          `ime_prezime`
                        )
                        VALUES
                          (
                            '?employee_id',
                            '?radno_mjesto',
                            '?mjesto_putovanja',
                            '?razlog_putovanja',
                            ?dat_poc_putovanja,
                            '?trajanje_putovanja',
                            '?prev_auto',
                            '?prev_vlauto',
                            '?prev_avion',
                            '?prev_bus',
                            '?prevoz_voz',
                            '?iznos_akontacije',
                            '?status_naloga',
                            '?troskovi_putovanja_knjizenje',
                            '?ime_prezime'
                          );
                         </SQL>
                strSQL = xmlSQL.Value
                strSQL = strSQL.Replace("?employee_id", pPutniNalog.employee_id.ToString)
                strSQL = strSQL.Replace("?radno_mjesto", CheckDBNull(pPutniNalog.radno_mjesto))
                strSQL = strSQL.Replace("?mjesto_putovanja", CheckDBNull(pPutniNalog.mjesto_putovanja))
                strSQL = strSQL.Replace("?razlog_putovanja", CheckDBNull(pPutniNalog.razlog_putovanja))
                strSQL = strSQL.Replace("?dat_poc_putovanja", dbDate(pPutniNalog.dat_poc_putovanja))
                strSQL = strSQL.Replace("?trajanje_putovanja", dbDouble(pPutniNalog.trajanje_putovanja))
                strSQL = strSQL.Replace("?prev_auto", dbBoolean(pPutniNalog.prev_auto))
                strSQL = strSQL.Replace("?prev_vlauto", dbBoolean(pPutniNalog.prev_vlauto))
                strSQL = strSQL.Replace("?prev_avion", dbBoolean(pPutniNalog.prev_avion))
                strSQL = strSQL.Replace("?prev_bus", dbBoolean(pPutniNalog.prev_autobus))
                strSQL = strSQL.Replace("?prevoz_voz", dbBoolean(pPutniNalog.prevoz_voz))
                strSQL = strSQL.Replace("?iznos_akontacije", dbDouble(pPutniNalog.iznos_akontacije))
                strSQL = strSQL.Replace("?status_naloga", pPutniNalog.status_naloga)
                strSQL = strSQL.Replace("?troskovi_putovanja_knjizenje", CheckDBNull(pPutniNalog.troskovi_putovanja_knjizenje))
                strSQL = strSQL.Replace("?ime_prezime", CheckDBNull(pPutniNalog.ime_prezime))

            Case Is > 0
                strSQL = "UPDATE PutniNalog SET radno_mjesto = '" & CheckDBNull(pPutniNalog.radno_mjesto) & "', " _
                            & "mjesto_putovanja = '" & CheckDBNull(pPutniNalog.mjesto_putovanja) & "', " _
                            & "razlog_putovanja = '" & CheckDBNull(pPutniNalog.razlog_putovanja) & "', " _
                            & "dat_poc_putovanja = " & dbDate(pPutniNalog.dat_poc_putovanja) & ", " _
                            & "trajanje_putovanja = " & dbDouble(pPutniNalog.trajanje_putovanja) & ", " _
                            & "prev_auto = " & dbBoolean(pPutniNalog.prev_auto) & ", " _
                            & "prev_vlauto = " & dbBoolean(pPutniNalog.prev_vlauto) & ", " _
                            & "prev_avion = " & dbBoolean(pPutniNalog.prev_avion) & ", " _
                            & "prev_autobus = " & dbBoolean(pPutniNalog.prev_autobus) & ", " _
                            & "prevoz_voz = " & dbBoolean(pPutniNalog.prevoz_voz) & ", " _
                            & "iznos_akontacije = " & dbDouble(pPutniNalog.iznos_akontacije) & ", " _
                            & "troskovi_putovanja_knjizenje = '" & CheckDBNull(pPutniNalog.troskovi_putovanja_knjizenje) & "' " _
                            & " WHERE putninalog_id=" & pPutniNalog.putninalog_id _
                            & " AND status_naloga=" & pPutniNalog.status_naloga & ";"

            Case Else
                strSQL = ""

        End Select


        Dim con As New MySqlClient.MySqlConnection(ConnectionString)
        Dim cmd As New MySqlClient.MySqlCommand(strSQL, con)

        Try
            con.Open()
            cmd.ExecuteNonQuery()
            retVal = True
            con.Close()
        Catch ex As Exception

        End Try

        Return retVal

    End Function
    Public Shared Sub DeletePutniNalog(ByVal putninalog_id As Integer)
        Dim con As New MySqlClient.MySqlConnection(ConnectionString)
        Dim sqlText As String = "DELETE FROM PutniNalog WHERE  putninalog_id =" & putninalog_id & ";"

        Dim cmd As New MySqlClient.MySqlCommand(sqlText, con)
        con.Open()
        cmd.ExecuteNonQuery()

        sqlText = "DELETE FROM PutniNalog_Obracun WHERE  putninalog_id =" & putninalog_id & ";"

        cmd.CommandText = sqlText

        cmd.ExecuteNonQuery()

        con.Close()
    End Sub

    Public Shared Sub savePutniNalog_Obracun_1(ByVal putninalog_id As Integer, ByVal line_id As Integer, _
    ByRef relacija As String, ByRef dat_polaska As String, ByRef vri_polaska As String, ByRef dat_povratka As String, _
    ByRef vri_povratka As String, ByRef broj_sati As String, ByRef broj_dnevnica As String, ByRef iznos_dnevnice As String, _
    ByRef ukupan_iznos As String)


        ukupan_iznos = "0"
        Try
            broj_sati = CStr(webTimeDiff(dat_polaska, vri_polaska, dat_povratka, vri_povratka))

            ' Izračunavanje broja dnevnica
            Dim dBrojDnevnica As Double = 0
            Dim restBrojSati As Double = CDbl(broj_sati) Mod 24
            Dim iBrojDnevnica As Double = Truncate(CDbl(broj_sati) / 24)

            If restBrojSati < 8 Then
                dBrojDnevnica = CDbl(iBrojDnevnica) + 0.0
            ElseIf restBrojSati >= 8 And restBrojSati < 12 Then
                dBrojDnevnica = CDbl(iBrojDnevnica) + 0.5
            ElseIf restBrojSati >= 12 Then
                dBrojDnevnica = iBrojDnevnica + 1.0
            End If


            broj_dnevnica = CStr(dBrojDnevnica)

            ukupan_iznos = CStr(CDbl(webDotToComma(broj_dnevnica)) * CDbl(webDotToComma(iznos_dnevnice)))

        Catch ex As Exception

        End Try



        Dim con As New MySqlClient.MySqlConnection(ConnectionString)
        Dim strSQL As String = "UPDATE PutniNalog_Obracun SET " _
                            & " relacija = '" & relacija & "' ," _
                            & " dat_polaska = " & dbDate(dat_polaska) & ", " _
                            & " vri_polaska = " & dbTime(vri_polaska) & ", " _
                            & " dat_povratka = " & dbDate(dat_povratka) & ", " _
                            & " vri_povratka = " & dbTime(vri_povratka) & ", " _
                            & " broj_sati = " & dbDouble(broj_sati) & ", " _
                            & " broj_dnevnica = " & dbDouble(broj_dnevnica) & ", " _
                            & " iznos_dnevnice = " & dbDouble(iznos_dnevnice) & ", " _
                            & " ukupan_iznos = " & dbDouble(ukupan_iznos) & " " _
                            & " WHERE putninalog_id = " & putninalog_id & " AND putninalog_oblast = 1 AND order_id=" & line_id & ";"

        Dim cmd As New MySqlClient.MySqlCommand(strSQL, con)
        con.Open()
        Try
            cmd.ExecuteNonQuery()
        Catch ex As Exception

        End Try

        con.Close()
    End Sub
    Public Shared Sub savePutniNalog_Obracun_2(ByVal putninalog_id As Integer, ByVal line_id As Integer, _
    ByRef relacija As String, ByRef ukupan_iznos As String, ByRef vrsta_prevoza As String, ByRef razred As String, ByRef iznos_karte As String, _
    ByRef naknada_km As String, ByRef troskovi_goriva As String, ByRef troskovi_parkinga As String, _
    ByRef ostali_troskovi As String)


        Dim con As New MySqlClient.MySqlConnection(ConnectionString)
        Dim strSQL As String = "UPDATE PutniNalog_Obracun SET " _
                            & " relacija = '" & relacija & "' ," _
                            & " ukupan_iznos = " & dbDouble(ukupan_iznos) & ", " _
                            & " vrsta_prevoza = '" & vrsta_prevoza & "', " _
                            & " razred = '" & razred & "', " _
                            & " iznos_karte = " & dbDouble(iznos_karte) & ", " _
                            & " naknada_km = " & dbDouble(naknada_km) & ", " _
                            & " troskovi_goriva = " & dbDouble(troskovi_goriva) & ", " _
                            & " troskovi_parkinga = " & dbDouble(troskovi_parkinga) & ", " _
                            & " ostali_troskovi = " & dbDouble(ostali_troskovi) & " " _
                            & " WHERE putninalog_id = " & putninalog_id & " AND putninalog_oblast = 2 AND order_id=" & line_id & ";"

        Dim cmd As New MySqlClient.MySqlCommand(strSQL, con)
        con.Open()
        Try
            cmd.ExecuteNonQuery()
        Catch ex As Exception

        End Try

        con.Close()
    End Sub
    Public Shared Sub savePutniNalog_Obracun_3(ByVal putninalog_id As Integer, ByVal line_id As Integer, _
    ByRef ukupan_iznos As String, ByRef obracun_ostalih_troskova As String)

        Dim con As New MySqlClient.MySqlConnection(ConnectionString)
        Dim strSQL As String = "UPDATE PutniNalog_Obracun SET " _
                            & " ukupan_iznos = " & dbDouble(ukupan_iznos) & ", " _
                            & " obracun_ostalih_troskova = '" & obracun_ostalih_troskova & "' " _
                            & " WHERE putninalog_id = " & putninalog_id & " AND putninalog_oblast = 3 AND order_id=" & line_id & ";"

        Dim cmd As New MySqlClient.MySqlCommand(strSQL, con)
        con.Open()
        Try
            cmd.ExecuteNonQuery()
        Catch ex As Exception

        End Try

        con.Close()
    End Sub
    Public Shared Sub savePutniNalog_Obracun_Rekapitulacija(ByVal putninalog_id As Integer, ByVal iznos_rek_a As String, ByVal iznos_rek_b As String, _
    ByVal iznos_rek_c As String, ByVal iznos_rek_ukupno As String, ByVal iznos_rek_akontacija As String, ByVal iznos_rek_razlika As String)

        Dim con As New MySqlClient.MySqlConnection(ConnectionString)
        Dim strSQL As String = "UPDATE PutniNalog SET " _
                            & " ukupnitroskovi_a = " & dbDouble(iznos_rek_a) & ", " _
                            & " ukupnitroskovi_b = " & dbDouble(iznos_rek_b) & ", " _
                            & " ukupnitroskovi_c = " & dbDouble(iznos_rek_c) & ", " _
                            & " iznos_obracuna = " & dbDouble(iznos_rek_ukupno) & ", " _
                            & " iznos_akontacije = " & dbDouble(iznos_rek_akontacija) & ", " _
                            & " iznos_razlika = " & dbDouble(iznos_rek_razlika) & " " _
                            & " WHERE putninalog_id = " & putninalog_id & ";"

        Dim cmd As New MySqlClient.MySqlCommand(strSQL, con)
        con.Open()
        Try
            cmd.ExecuteNonQuery()
        Catch ex As Exception

        End Try

        con.Close()
    End Sub

    Public Shared Function getPutniNalog() As Integer
        Dim con As New MySqlClient.MySqlConnection(ConnectionString)

        Dim sqlText As String = "SELECT LAST_INSERT_ID() AS PNID;"
        Dim cmd As New MySqlClient.MySqlCommand(sqlText, con)

        Dim _retVal As Integer = -1

        con.Open()
        Dim rd As MySqlClient.MySqlDataReader = cmd.ExecuteReader()

        Try

            Do While rd.Read
                _retVal = CheckDBNull(rd("PNID"))

            Loop
        Catch ex As Exception

        End Try
        con.Close()

        Return _retVal

    End Function
    Public Shared Sub getPutniNalog(ByVal putninalog_id As Integer, ByRef status_naloga As Integer, _
    ByRef dat_poc_putovanja As String, ByRef mjesto_putovanja As String, ByRef radno_mjesto As String, _
    ByRef odobrena_akontacija As Boolean, ByRef razlog_putovanja As String, ByRef trajanje_putovanja As String, _
    ByRef troskovi_putovanja_knjizenje As String, _
    ByRef prev_auto As Boolean, ByRef prev_vlauto As Boolean, ByRef prev_avion As Boolean, ByRef prev_autobus As Boolean, _
    ByRef prevoz_voz As Boolean, ByRef iznos_akontacije As String)


        Dim con As New MySqlClient.MySqlConnection(ConnectionString)
        Dim SQL As String = "SELECT putninalog_id, employee_id, radno_mjesto, mjesto_putovanja, razlog_putovanja, " _
        & " dat_poc_putovanja, trajanje_putovanja, prev_auto, prev_vlauto, prev_avion, prev_autobus, prevoz_voz, odobrena_akontacija, " _
        & " iznos_akontacije, troskovi_putovanja_knjizenje, status_naloga, date_added FROM PutniNalog " _
        & " WHERE PutniNalog.putninalog_id=" & putninalog_id & " ;"

        Dim cmd As New MySqlClient.MySqlCommand(SQL, con)

        con.Open()

        Dim rd As MySqlClient.MySqlDataReader = cmd.ExecuteReader()
        Dim tmpDatum As MySql.Data.Types.MySqlDateTime

        Try
            Do While rd.Read
                mjesto_putovanja = CheckDBNull(rd("mjesto_putovanja"))
                radno_mjesto = CheckDBNull(rd("radno_mjesto"))
                odobrena_akontacija = CheckDBNullBoolean(rd("odobrena_akontacija"))
                status_naloga = CheckDBNullBoolean(rd("status_naloga"))
                razlog_putovanja = CheckDBNull(rd("razlog_putovanja"))
                trajanje_putovanja = webCommaToDot(CheckDBNull(rd("trajanje_putovanja")))
                troskovi_putovanja_knjizenje = CheckDBNull(rd("troskovi_putovanja_knjizenje"))
                prev_auto = CheckDBNullBoolean(rd("prev_auto"))
                prev_vlauto = CheckDBNullBoolean(rd("prev_vlauto"))
                prev_avion = CheckDBNullBoolean(rd("prev_avion"))
                prev_autobus = CheckDBNullBoolean(rd("prev_autobus"))
                prevoz_voz = CheckDBNullBoolean(rd("prevoz_voz"))
                iznos_akontacije = webCommaToDot(CheckDBNull(rd("iznos_akontacije")))

                tmpDatum = rd("dat_poc_putovanja")
                dat_poc_putovanja = webMySqlDate(tmpDatum)

            Loop
        Catch ex As Exception

        End Try

        rd.Close()
        con.Close()

    End Sub

    Public Shared Sub getPutniNalog_Obracun_1(ByVal putninalog_id As Integer, ByVal line_id As Integer, _
        ByRef relacija As String, ByRef dat_polaska As String, ByRef vri_polaska As String, ByRef dat_povratka As String, _
        ByRef vri_povratka As String, ByRef broj_sati As String, ByRef broj_dnevnica As String, ByRef iznos_dnevnice As String, _
        ByRef ukupan_iznos As String)

        Dim con As New MySqlClient.MySqlConnection(ConnectionString)
        Dim SQL As String = "SELECT relacija, dat_polaska, vri_polaska, dat_povratka, vri_povratka, " _
                          & "broj_sati, broj_dnevnica, iznos_dnevnice, ukupan_iznos, vrsta_prevoza, razred, " _
                          & "iznos_karte, naknada_km, troskovi_goriva,troskovi_parkinga, ostali_troskovi, " _
                          & "obracun_ostalih_troskova " _
                          & "FROM PutniNalog_Obracun WHERE putninalog_id=" & putninalog_id.ToString & " AND putninalog_oblast=1 AND order_id = " & line_id.ToString & ";"

        Dim cmd As New MySqlClient.MySqlCommand(SQL, con)
        Dim tmpDatum As MySql.Data.Types.MySqlDateTime

        con.Open()
        Dim rd As MySqlClient.MySqlDataReader = cmd.ExecuteReader()
        Try

            Do While rd.Read
                relacija = CheckDBNull(rd("relacija"))
                Try

                    tmpDatum = CheckDBNull(rd("dat_polaska"))
                    dat_polaska = webMySqlDate(tmpDatum)
                Catch ex As Exception
                    dat_polaska = ""
                End Try

                'dat_polaska = CheckDBNull(rd("dat_polaska"))
                vri_polaska = webTime2(CheckDBNull(rd("vri_polaska")))

                Try
                    tmpDatum = CheckDBNull(rd("dat_povratka"))
                    dat_povratka = webMySqlDate(tmpDatum)
                Catch ex As Exception
                    dat_povratka = ""
                End Try

                Try
                    vri_povratka = webTime2(CheckDBNull(rd("vri_povratka")))
                    broj_sati = webDouble(CheckDBNull(rd("broj_sati")))
                    broj_dnevnica = webDouble(CheckDBNull(rd("broj_dnevnica")))
                    iznos_dnevnice = webDouble(CheckDBNull(rd("iznos_dnevnice")))
                    ukupan_iznos = webDouble(CheckDBNull(rd("ukupan_iznos")))
                Catch ex As Exception

                End Try

            Loop
        Catch ex As Exception

        End Try

        rd.Close()
        con.Close()

        Try
            If (dat_polaska <> "") And (dat_polaska <> "null") Then dat_polaska = webDate(dat_polaska) Else dat_polaska = ""
            If (dat_povratka <> "") And (dat_povratka <> "null") Then dat_povratka = webDate(dat_povratka) Else dat_povratka = ""
            If (vri_polaska <> "") And (vri_polaska <> "null") Then vri_polaska = webTime(vri_polaska) Else vri_polaska = ""
            If (vri_povratka <> "") And (vri_povratka <> "null") Then vri_povratka = webTime(vri_povratka) Else vri_povratka = ""
        Catch ex As Exception

        End Try


    End Sub
    Public Shared Sub getPutniNalog_Obracun_2(ByVal putninalog_id As Integer, ByVal line_id As Integer, _
    ByRef relacija As String, ByRef ukupan_iznos As String, ByRef vrsta_prevoza As String, ByRef razred As String, ByRef iznos_karte As String, _
    ByRef naknada_km As String, ByRef troskovi_goriva As String, ByRef troskovi_parkinga As String, _
    ByRef ostali_troskovi As String)


        Dim con As New MySqlClient.MySqlConnection(ConnectionString)
        Dim SQL As String = "SELECT relacija, dat_polaska, vri_polaska, dat_povratka, vri_povratka, " _
                          & "broj_sati, broj_dnevnica, iznos_dnevnice, ukupan_iznos, vrsta_prevoza, razred, " _
                          & "iznos_karte, naknada_km, troskovi_goriva, troskovi_parkinga, ostali_troskovi, " _
                          & "obracun_ostalih_troskova " _
                          & "FROM PutniNalog_Obracun WHERE putninalog_id=" & putninalog_id.ToString & " AND putninalog_oblast=2 AND order_id = " & line_id.ToString & ";"

        Dim cmd As New MySqlClient.MySqlCommand(SQL, con)

        con.Open()

        Dim rd As MySqlClient.MySqlDataReader = cmd.ExecuteReader()
        Do While rd.Read
            relacija = CheckDBNull(rd("relacija"))
            ukupan_iznos = webDouble(CheckDBNull(rd("ukupan_iznos")))
            vrsta_prevoza = CheckDBNull(rd("vrsta_prevoza"))
            razred = CheckDBNull(rd("razred"))
            iznos_karte = webDouble(CheckDBNull(rd("iznos_karte")))
            naknada_km = webDouble(CheckDBNull(rd("naknada_km")))
            troskovi_goriva = webDouble(CheckDBNull(rd("troskovi_goriva")))
            troskovi_parkinga = webDouble(CheckDBNull(rd("troskovi_parkinga")))
            ostali_troskovi = webDouble(CheckDBNull(rd("ostali_troskovi")))
        Loop
        rd.Close()
        con.Close()

    End Sub
    Public Shared Sub getPutniNalog_Obracun_3(ByVal putninalog_id As Integer, ByVal line_id As Integer, _
    ByRef ukupan_iznos As String, ByRef obracun_ostalih_troskova As String)


        Dim con As New MySqlClient.MySqlConnection(ConnectionString)
        Dim strSQL As String = "SELECT relacija, dat_polaska, vri_polaska, dat_povratka, vri_povratka, " _
                          & "broj_sati, broj_dnevnica, iznos_dnevnice, ukupan_iznos, vrsta_prevoza, razred, " _
                          & "iznos_karte, naknada_km, troskovi_goriva, troskovi_parkinga, ostali_troskovi, " _
                          & "obracun_ostalih_troskova " _
                          & "FROM PutniNalog_Obracun WHERE putninalog_id=" & putninalog_id.ToString & " AND putninalog_oblast=3 AND order_id = " & line_id.ToString & ";"

        Dim cmd As New MySqlClient.MySqlCommand(strSQL, con)

        con.Open()

        Dim rd As MySqlClient.MySqlDataReader = cmd.ExecuteReader()
        Do While rd.Read
            ukupan_iznos = webDouble(CheckDBNull(rd("ukupan_iznos")))
            obracun_ostalih_troskova = (CheckDBNull(rd("obracun_ostalih_troskova")))
        Loop
        rd.Close()
        con.Close()

    End Sub
    Public Shared Sub getPutniNalog_Obracun_rekapitulacija(ByVal putninalog_id As Integer, ByRef iznos_rek_1 As String, ByRef iznos_rek_2 As String, _
    ByRef iznos_rek_3 As String, ByRef iznos_rek_ukupno As String, ByRef iznos_rek_akontacija As String, ByRef iznos_rek_razlika As String)


        Dim con As New MySqlClient.MySqlConnection(ConnectionString)
        Dim strSQL As String = "SELECT * " _
                          & "FROM PutniNalog WHERE putninalog_id=" & putninalog_id.ToString & " ;"

        Dim cmd As New MySqlClient.MySqlCommand(strSQL, con)

        con.Open()

        Dim rd As MySqlClient.MySqlDataReader = cmd.ExecuteReader()
        Do While rd.Read
            iznos_rek_1 = CheckDBNull(rd("ukupnitroskovi_a"))
            iznos_rek_2 = CheckDBNull(rd("ukupnitroskovi_b"))
            iznos_rek_3 = CheckDBNull(rd("ukupnitroskovi_c"))
            iznos_rek_ukupno = CheckDBNull(rd("iznos_obracuna"))
            iznos_rek_akontacija = CheckDBNull(rd("iznos_akontacije"))
            iznos_rek_razlika = CheckDBNull(rd("iznos_razlika"))
        Loop
        rd.Close()
        con.Close()

    End Sub

    Public Shared Sub odobriPutniNalog(ByVal putninalog_id As Integer, ByVal employee_id As Integer)

        Dim con As New MySqlClient.MySqlConnection(ConnectionString)
        Dim strSQL As String = "UPDATE PutniNalog SET " _
                            & " status_naloga = " & nalogStatus.Odobren & "  WHERE putninalog_id = " & putninalog_id & " AND iznos_akontacije>0;"
        Dim strSQL2 As String = "UPDATE PutniNalog SET " _
                            & " status_naloga = " & nalogStatus.Aktivan & "  WHERE putninalog_id = " & putninalog_id & " AND iznos_akontacije=0;"

        Dim cmd As New MySqlClient.MySqlCommand(strSQL, con)
        con.Open()
        Try
            cmd.ExecuteNonQuery()
            cmd.CommandText = strSQL2
            cmd.ExecuteNonQuery()

        Catch ex As Exception

        End Try

        con.Close()

        logOdobravanjaPutniNalog(putninalog_id, employee_id, nalogStatus.Odobren)
    End Sub
    Public Shared Sub odobriPutniNalog_Storno(ByVal putninalog_id As Integer, ByVal employee_id As Integer)

        Dim con As New MySqlClient.MySqlConnection(ConnectionString)
        Dim strSQL As String = "UPDATE PutniNalog SET " _
                            & " status_naloga = " & nalogStatus.Otvoren & "  WHERE putninalog_id = " & putninalog_id & " ;"

        Dim cmd As New MySqlClient.MySqlCommand(strSQL, con)
        con.Open()
        Try
            cmd.ExecuteNonQuery()
        Catch ex As Exception

        End Try

        con.Close()

        logOdobravanjaPutniNalog(putninalog_id, employee_id, nalogStatus.Otvoren)
    End Sub
    Public Shared Sub popuniPutniNalog(ByVal putninalog_id As Integer, ByVal employee_id As Integer)

        Dim con As New MySqlClient.MySqlConnection(ConnectionString)
        Dim strSQL As String = "UPDATE PutniNalog SET " _
                            & " status_naloga = " & nalogStatus.Popunjen & "  WHERE putninalog_id = " & putninalog_id & " ;"

        Dim cmd As New MySqlClient.MySqlCommand(strSQL, con)
        con.Open()
        Try
            cmd.ExecuteNonQuery()
        Catch ex As Exception

        End Try

        con.Close()

        logOdobravanjaPutniNalog(putninalog_id, employee_id, nalogStatus.Popunjen)
    End Sub
    Public Shared Sub popuniPutniNalog_Storno(ByVal putninalog_id As Integer, ByVal employee_id As Integer)

        Dim con As New MySqlClient.MySqlConnection(ConnectionString)
        Dim strSQL As String = "UPDATE PutniNalog SET " _
                            & " status_naloga = " & nalogStatus.Aktivan & "  WHERE putninalog_id = " & putninalog_id & " ;"

        Dim cmd As New MySqlClient.MySqlCommand(strSQL, con)
        con.Open()
        Try
            cmd.ExecuteNonQuery()
        Catch ex As Exception

        End Try

        con.Close()

        logOdobravanjaPutniNalog(putninalog_id, employee_id, nalogStatus.Aktivan)
    End Sub
    Public Shared Sub provjeriPutniNalog(ByVal putninalog_id As Integer, ByVal employee_id As Integer)

        Dim con As New MySqlClient.MySqlConnection(ConnectionString)
        Dim strSQL As String = "UPDATE PutniNalog SET " _
                            & " status_naloga = " & nalogStatus.Provjeren & "  WHERE putninalog_id = " & putninalog_id & " ;"

        Dim cmd As New MySqlClient.MySqlCommand(strSQL, con)
        con.Open()
        Try
            cmd.ExecuteNonQuery()
        Catch ex As Exception

        End Try

        con.Close()

        logOdobravanjaPutniNalog(putninalog_id, employee_id, nalogStatus.Provjeren)
    End Sub
    Public Shared Sub aktivirajPutniNalog(ByVal putninalog_id As Integer, ByVal employee_id As Integer)

        Dim con As New MySqlClient.MySqlConnection(ConnectionString)
        Dim strSQL As String = "UPDATE PutniNalog SET " _
                            & " status_naloga = " & nalogStatus.Aktivan & "  WHERE putninalog_id = " & putninalog_id & " ;"

        Dim cmd As New MySqlClient.MySqlCommand(strSQL, con)
        con.Open()
        Try
            cmd.ExecuteNonQuery()
        Catch ex As Exception

        End Try

        con.Close()

        logOdobravanjaPutniNalog(putninalog_id, employee_id, nalogStatus.Aktivan)
    End Sub
    Public Shared Sub obracunajPutniNalog(ByVal putninalog_id As Integer, ByVal employee_id As Integer)

        Dim con As New MySqlClient.MySqlConnection(ConnectionString)
        Dim strSQL As String = "UPDATE PutniNalog SET " _
                            & " status_naloga = " & nalogStatus.Obracunat & "  WHERE putninalog_id = " & putninalog_id & " ;"

        Dim cmd As New MySqlClient.MySqlCommand(strSQL, con)
        con.Open()
        Try
            cmd.ExecuteNonQuery()
        Catch ex As Exception

        End Try

        con.Close()

        logOdobravanjaPutniNalog(putninalog_id, employee_id, nalogStatus.Obracunat)
    End Sub
    Public Shared Sub opravdajPutniNalog(ByVal putninalog_id As Integer, ByVal employee_id As Integer)

        Dim con As New MySqlClient.MySqlConnection(ConnectionString)
        Dim strSQL As String = "UPDATE PutniNalog SET " _
                            & " status_naloga = " & nalogStatus.Opravdan & "  WHERE putninalog_id = " & putninalog_id & " ;"

        Dim cmd As New MySqlClient.MySqlCommand(strSQL, con)
        con.Open()
        Try
            cmd.ExecuteNonQuery()
        Catch ex As Exception

        End Try

        con.Close()

        logOdobravanjaPutniNalog(putninalog_id, employee_id, nalogStatus.Opravdan)
    End Sub

    Public Shared Sub logOdobravanjaPutniNalog(ByVal putninalog_id As Integer, ByVal employee_id As Integer, ByVal status_id As Integer)

        Dim con As New MySqlClient.MySqlConnection(ConnectionString)
        Dim strSQL As String = "INSERT INTO putninalog_odobravanje (putninalog_id, employee_id, status_id,	date_added) " _
                                & " VALUES (" & putninalog_id & ", " & employee_id & ", " & status_id & ", now());"

        Dim cmd As New MySqlClient.MySqlCommand(strSQL, con)
        con.Open()
        Try
            cmd.ExecuteNonQuery()
        Catch ex As Exception

        End Try

        Select Case status_id

            Case 2 ' odobren
                strSQL = "UPDATE putninalog pn, evd_employees e SET pn_odobrio = CONCAT(e.LastName,' ',e.FirstName) WHERE putninalog_id = " & putninalog_id.ToString & " AND e.EmployeeID=" & employee_id.ToString & "  ;"
            Case 7 ' obracun provjerio
                strSQL = "UPDATE putninalog pn, evd_employees e SET pn_isplatio = CONCAT(e.LastName,' ',e.FirstName) WHERE putninalog_id = " & putninalog_id.ToString & " AND e.EmployeeID=" & employee_id.ToString & "  ;"
            Case 5 ' obracunat
                strSQL = "UPDATE putninalog pn, evd_employees e SET pn_obracunao = CONCAT(e.LastName,' ',e.FirstName) WHERE putninalog_id = " & putninalog_id.ToString & " AND e.EmployeeID=" & employee_id.ToString & "  ;"
            Case 6 ' likvidiran
                strSQL = "UPDATE putninalog pn, evd_employees e SET pn_likvidirao = CONCAT(e.LastName,' ',e.FirstName) WHERE putninalog_id = " & putninalog_id.ToString & " AND e.EmployeeID=" & employee_id.ToString & "  ;"
        End Select

        Try
            cmd.CommandText = strSQL
            cmd.ExecuteNonQuery()
        Catch ex As Exception

        End Try


        con.Close()
    End Sub

    Public Shared Sub getPutniNalozi_Likvidatura(ByRef Repeater1 As Repeater, ByVal statusNaloga1 As nalogStatus, ByVal statusNaloga2 As nalogStatus, ByVal statusNaloga3 As nalogStatus)
        Dim con As New MySqlClient.MySqlConnection(ConnectionString)

        Dim sqlText As String = "SELECT PN.putninalog_id, PN.employee_id, PN.radno_mjesto, PN.mjesto_putovanja, PN.razlog_putovanja, " _
        & " PN.dat_poc_putovanja, PN.trajanje_putovanja, PN.prev_auto, PN.prev_vlauto, " _
        & "PN.prev_avion, PN.prev_autobus, PN.prevoz_voz, PN.odobrena_akontacija, PN.iznos_akontacije, PN.status_naloga, " _
        & "PN.date_added, PN.troskovi_putovanja_knjizenje, PN.iznos_obracuna, PN.iznos_razlika, PN.ime_prezime,  " _
        & "PN_S.status_text FROM PutniNalog As PN INNER JOIN PutniNalog_Status As PN_S ON PN.status_naloga = PN_S.status_id " _
        & "WHERE PN.status_naloga=" & statusNaloga1 & " OR PN.status_naloga=" & statusNaloga2 & " OR PN.status_naloga=" & statusNaloga3 & " Order By PN.dat_poc_putovanja Desc;"

        Dim cmd As New MySqlClient.MySqlCommand(sqlText, con)

        con.Open()
        Dim rd As MySqlClient.MySqlDataReader = cmd.ExecuteReader()
        Repeater1.DataSource = rd
        Repeater1.DataBind()
        con.Close()
    End Sub

    Public Shared Sub getPutniNalozi_Unos(ByRef Repeater1 As Repeater, ByVal statusNaloga1 As nalogStatus, ByVal statusNaloga2 As nalogStatus, ByVal statusNaloga3 As nalogStatus, _
                            ByRef EmployeeID As Integer)
        Dim con As New MySqlClient.MySqlConnection(ConnectionString)

        Dim sqlText As String = "SELECT PN.putninalog_id, PN.employee_id, PN.radno_mjesto, PN.mjesto_putovanja, PN.razlog_putovanja, " _
        & " PN.dat_poc_putovanja, PN.trajanje_putovanja, PN.prev_auto, PN.prev_vlauto, " _
        & "PN.prev_avion, PN.prev_autobus, PN.prevoz_voz, PN.odobrena_akontacija, PN.iznos_akontacije, PN.status_naloga, " _
        & "PN.date_added, PN.troskovi_putovanja_knjizenje, PN.iznos_obracuna, PN.iznos_razlika, PN.ime_prezime,  " _
        & "PN_S.status_text FROM PutniNalog As PN INNER JOIN PutniNalog_Status As PN_S ON PN.status_naloga = PN_S.status_id " _
        & "WHERE (PN.employee_id = " & EmployeeID & ") AND (PN.status_naloga=" & statusNaloga1 & " OR PN.status_naloga=" & statusNaloga2 & " OR PN.status_naloga=" & statusNaloga3 & ") Order By PN.status_naloga, PN.dat_poc_putovanja Desc;"

        Dim cmd As New MySqlClient.MySqlCommand(sqlText, con)

        con.Open()
        Dim rd As MySqlClient.MySqlDataReader = cmd.ExecuteReader()
        Repeater1.DataSource = rd
        Repeater1.DataBind()
        con.Close()
    End Sub

    Public Shared Sub getPutniNalozi_Odobravanje(ByRef Repeater1 As Repeater, ByVal statusNaloga1 As nalogStatus, ByVal statusNaloga2 As nalogStatus, ByVal statusNaloga3 As nalogStatus, _
                        ByRef EmployeeID As Integer, ByVal curRole As String)
        Dim con As New MySqlClient.MySqlConnection(ConnectionString)

        Dim EmployeesList As String = ClsPutniNalozi.Uposlenici_ListaByRole(curRole, EmployeeID)

        Dim sqlText As String = "SELECT PN.putninalog_id, PN.employee_id, PN.radno_mjesto, PN.mjesto_putovanja, PN.razlog_putovanja, " _
        & " PN.dat_poc_putovanja, PN.trajanje_putovanja, PN.prev_auto, PN.prev_vlauto, " _
        & "PN.prev_avion, PN.prev_autobus, PN.prevoz_voz, PN.odobrena_akontacija, PN.iznos_akontacije, PN.status_naloga, " _
        & "PN.date_added, PN.troskovi_putovanja_knjizenje, PN.iznos_obracuna, PN.iznos_razlika, PN.ime_prezime,  " _
        & "PN_S.status_text FROM PutniNalog As PN INNER JOIN PutniNalog_Status As PN_S ON PN.status_naloga = PN_S.status_id " _
        & "WHERE (PN.employee_id IN (" & EmployeesList & ")) AND (PN.status_naloga=" & statusNaloga1 & " OR PN.status_naloga=" & statusNaloga2 & " OR PN.status_naloga=" & statusNaloga3 & ") Order By PN.status_naloga, PN.dat_poc_putovanja Desc;"

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

    Public Shared Sub getPutniNalozi_OdobravanjeSaNulom(ByRef Repeater1 As Repeater, ByVal statusNaloga1 As nalogStatus, ByVal statusNaloga2 As nalogStatus, ByVal statusNaloga3 As nalogStatus, _
                     ByRef EmployeeID As Integer, ByVal curRole As String)
        Dim con As New MySqlClient.MySqlConnection(ConnectionString)

        Dim EmployeesList As String = ClsPutniNalozi.Uposlenici_ListaByRole(curRole, EmployeeID)

        Dim sqlText As String = "SELECT PN.putninalog_id, PN.employee_id, PN.radno_mjesto, PN.mjesto_putovanja, PN.razlog_putovanja, " _
        & " PN.dat_poc_putovanja, PN.trajanje_putovanja, PN.prev_auto, PN.prev_vlauto, " _
        & "PN.prev_avion, PN.prev_autobus, PN.prevoz_voz, PN.odobrena_akontacija, PN.iznos_akontacije, PN.status_naloga, " _
        & "PN.date_added, PN.troskovi_putovanja_knjizenje, PN.iznos_obracuna, PN.iznos_razlika, PN.ime_prezime,  " _
        & "PN_S.status_text FROM PutniNalog As PN INNER JOIN PutniNalog_Status As PN_S ON PN.status_naloga = PN_S.status_id " _
        & "WHERE (PN.employee_id IN (" & EmployeesList & ")) AND (PN.status_naloga=" & statusNaloga1 & " OR PN.status_naloga=" & statusNaloga2 & " OR (PN.status_naloga=" & statusNaloga3 & " AND PN.iznos_akontacije=0)) Order By PN.status_naloga, PN.dat_poc_putovanja Desc;"

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

    Public Shared Function isAllowed_CreatePutniNalog(ByVal EmployeeID As Integer) As Boolean
        Dim con As New MySqlClient.MySqlConnection(ConnectionString)

        Dim sqlText As String = "SELECT COUNT(pn.putninalog_id) AS MAX_NUM_PN_OTV FROM plc_portal.putninalog AS pn " _
                            & " INNER JOIN plc_portal.putninalog_status AS pns ON (pn.status_naloga = pns.status_id) " _
                            & "WHERE (pn.employee_id = " & EmployeeID.ToString & ") AND pns.status_text = 'otvoren';"

        Dim cmd As New MySqlClient.MySqlCommand(sqlText, con)

        con.Open()
        Dim rd As MySqlClient.MySqlDataReader = cmd.ExecuteReader()
        Dim brojOtvPutNal As Integer
        Try
            While rd.Read
                brojOtvPutNal = rd.Item("MAX_NUM_PN_OTV")
            End While
        Catch ex As Exception

        End Try
        rd.Close()

        sqlText = "SELECT adm_value AS MAX_NUM_PN_OTV FROM  adm_configuration WHERE adm_parameter = 'MAX_NUM_PN_OTV';"
        cmd.CommandText = sqlText
        rd = cmd.ExecuteReader
        Dim brojOtvPutNal_Parametar As Integer
        Try
            While rd.Read
                brojOtvPutNal_Parametar = CInt(rd.Item("MAX_NUM_PN_OTV"))
            End While
        Catch ex As Exception

        End Try

        rd.Close()

        If brojOtvPutNal - brojOtvPutNal_Parametar >= 0 Then
            Return False
        Else
            Return True

        End If

        con.Close()
    End Function
    Public Shared Function isAllowed_DatPocPutovanja(ByVal DatPocPutovanja As String) As Boolean
        Dim isAllowed As Boolean = False

        If getAdmParameter("LIMIT_POC_DAT_PN") = "0" Then
            isAllowed = True
            Return True
        Else
            isAllowed = False
        End If



        Dim chkDate As Date
        Try
            chkDate = New Date(CInt(DatPocPutovanja.Substring(6, 4)), CInt(DatPocPutovanja.Substring(3, 2)), _
                          CInt(DatPocPutovanja.Substring(0, 2)))
            Dim dateNow As Date = Date.Now
            Dim chkDateDiff As Integer = DateDiff(DateInterval.Day, dateNow, chkDate)

            If chkDateDiff >= 0 Then
                Return True
            Else
                Return False

            End If
        Catch ex As Exception
            Return False
        End Try

    End Function

    Public Shared Function getAdmParameter(ByVal admParametar As String) As String
        Dim con As New MySqlClient.MySqlConnection(ConnectionString)

        Dim sqlText As String = ""

        Dim cmd As New MySqlClient.MySqlCommand(sqlText, con)

        con.Open()
        Dim rd As MySqlClient.MySqlDataReader


        sqlText = "SELECT adm_value AS " & admParametar & " FROM  adm_configuration WHERE adm_parameter = '" & admParametar & "';"
        cmd.CommandText = sqlText
        rd = cmd.ExecuteReader
        Dim retParametar As Integer
        Try
            While rd.Read
                retParametar = CInt(rd.Item(admParametar))
            End While
        Catch ex As Exception

        End Try

        rd.Close()

        con.Close()
        Return retParametar

    End Function
End Class
