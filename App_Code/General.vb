Imports Microsoft.VisualBasic
Imports System.Web.Configuration
Imports System.Data.SqlClient
Imports System.Data
Imports MySql.Data
Imports System.Math
Imports System.Web.Security
Imports System


Public Module General
    Public ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString
    'Public dbConnection As MySql.Data.MySqlClient.MySqlConnection

    Public defUserPefix As String = "bhrt"
    Public defDummyEmail As String = "bhrt@bhrt.ba"


    Enum eRoles
        rukovodilac = 1
        uposlenik = 2
        administrator = 3
        blagajna = 4
        likvidatura = 5
        evidencija = 6
        pravna = 7
        uprava = 8
        direkcija = 9
    End Enum

    Enum enumObjectType
        StrType = 0
        IntType = 1
        DblType = 2
    End Enum

    Public Enum Obracuni_e As Integer
        Svi = 0
        Nezavrseni = 1
        Tekuci = 2
        Kontrolni = 10
        Finalni = 11
    End Enum

    Public Enum ObustaveVrsta_e As Integer
        Sve = 0
        Jednokratne = 1
        Jednokratne_sa_ponavljanjem = 2
        Krediti = 3
        Ostale = 4
    End Enum

    Public Enum FormInit_e As Integer
        Binding = 0
        List = 1
    End Enum

    Public Enum Organizacija_e As Integer
        Sve = 0
        SamoObracun = 1
        SamoUposlenici = 2
    End Enum

    Public Enum StrucnaSprema_e As Integer
        SamoSprema = 0
        Kombinacije = 1
    End Enum

    Enum nalogStatus
        Otvoren = 1
        Odobren = 2
        Aktivan = 3
        Popunjen = 4
        Provjeren = 5
        Obracunat = 6
        Opravdan = 7
        Arhiviran = 8
    End Enum

    Enum evidencijaOdobrenje
        Podnesen = 1
        Odobren = 2
        Kontrolisan = 3
        Locked = 4
    End Enum

    Function CheckDBNull(ByVal obj As Object, _
    Optional ByVal ObjectType As enumObjectType = enumObjectType.StrType) As Object
        Dim objReturn As Object
        objReturn = obj
        If ObjectType = enumObjectType.StrType And IsDBNull(obj) Then
            objReturn = ""
        ElseIf ObjectType = enumObjectType.IntType And IsDBNull(obj) Then
            objReturn = 0
        ElseIf ObjectType = enumObjectType.DblType And IsDBNull(obj) Then
            objReturn = 0.0
        ElseIf IsDBNull(obj) Then
            objReturn = Nothing
        End If
        Return objReturn
    End Function

    Function CheckDBNullBoolean(ByVal obj As Object, _
    Optional ByVal ObjectType As enumObjectType = enumObjectType.StrType) As Object
        Dim objReturn As Object
        objReturn = obj
        If IsDBNull(obj) Then
            objReturn = False
        End If
        Return objReturn
    End Function

    Function CheckStatus(ByVal statusNaloga As Integer) As String
        Select Case statusNaloga
            Case 1
                Return "Otvoren"
            Case 2
                Return "Odobren"
            Case 3
                Return "Aktivan"
            Case 4
                Return "Popunjen"
            Case 5
                Return "Provjeren"
            Case 6
                Return "Obraèunat"
            Case 7
                Return "Opravdan"
            Case 8
                Return "Arhiviran"
        End Select

        Return "Nepoznat"
    End Function


#Region "dbCheck"

    Function dbBoolean(ByVal fldBoolean As Boolean) As String
        If fldBoolean Then
            Return "1"
        End If

        Return "0"
    End Function

    Function dbDate(ByVal fldDate As String) As String
        If Trim(fldDate) = "" Then Return "null"
        Dim chkDate As Date
        Try
            chkDate = New Date(CInt(fldDate.Substring(6, 4)), _
                                      CInt(fldDate.Substring(3, 2)), _
                                      CInt(fldDate.Substring(0, 2)))
            Return "'" & fldDate.Substring(6, 4) & "/" & fldDate.Substring(3, 2) & "/" & fldDate.Substring(0, 2) & "'"
        Catch ex As Exception
            Return "null"
        End Try

    End Function
    Function dbIsDate(ByVal fldDate As String) As Boolean
        Dim chkDate As Date
        If Trim(fldDate) = "" Then Return True
        Try
            chkDate = New Date(CInt(fldDate.Substring(6, 4)), _
                                      CInt(fldDate.Substring(3, 2)), _
                                      CInt(fldDate.Substring(0, 2)))
            Return True
        Catch ex As Exception
            Return False
        End Try

    End Function

    Function dbTime(ByVal fldDate As String) As String
        If Trim(fldDate) = "" Then Return "null"
        Dim chkDate As DateTime
        Try
            chkDate = CType("#" & fldDate & ":00#", DateTime)
            Return "'" & fldDate & ":00'"
        Catch ex As Exception
            Return "null"
        End Try

    End Function
    Function dbIsTime(ByVal fldDate As String) As Boolean
        If Trim(fldDate) = "" Then Return True
        Dim chkDate As DateTime
        Try
            chkDate = CType("#" & fldDate & ":00#", DateTime)
            Return True
        Catch ex As Exception
            Return False
        End Try

    End Function

    Function dbInteger(ByVal fldInteger As String) As String
        If Trim(fldInteger) = "" Then Return "null"
        Dim chkInteger As Integer
        Try
            fldInteger = Replace(fldInteger, ".", ",")
            chkInteger = CDbl(fldInteger)
            Return fldInteger
        Catch ex As Exception
            Return "null"
        End Try

    End Function
    Function dbIsInteger(ByVal fldInteger As String) As Boolean
        Dim chkInteger As Integer
        If Trim(fldInteger) = "" Then Return True
        Try
            fldInteger = Replace(fldInteger, ".", ",")
            chkInteger = CDbl(fldInteger)
            Return True
        Catch ex As Exception
            Return False
        End Try

    End Function

    Function dbDouble(ByVal fldDouble As String) As String
        Dim chkdouble As Double
        Try
            fldDouble = Replace(fldDouble, ".", ",")
            chkdouble = CDbl(fldDouble)
            chkdouble = Round(chkdouble, 2)
            Return Replace(chkdouble.ToString, ",", ".")
        Catch ex As Exception
            Return "null"
        End Try

    End Function
    Function dbIsDouble(ByVal fldInteger As String) As Boolean
        Dim chkInteger As Integer
        If Trim(fldInteger) = "" Then Return True
        Try
            chkInteger = CDbl(fldInteger)
            Return True
        Catch ex As Exception
            Return False
        End Try

    End Function

#End Region

    Function webDate(ByVal fldDate As Date) As String
        Dim chkDate As String = ""
        'If fldDate =  Then Return ""
        Try
            chkDate = fldDate.ToString("dd.MM.yyyy")
        Catch ex As Exception

        End Try
        Return chkDate
    End Function

    Function webMySqlDate(ByVal fldDate As MySql.Data.Types.MySqlDateTime) As String
        Dim chkDate As String = ""
        Dim tmpDate As New System.DateTime(fldDate.Year, fldDate.Month, fldDate.Day)
        'If fldDate =  Then Return ""
        Try

            chkDate = tmpDate.ToString("dd.MM.yyyy")
        Catch ex As Exception

        End Try
        Return chkDate
    End Function

    Function webTime(ByVal fldDate As Date) As String
        Dim chkDate As String = ""
        'If fldDate = "null" Then Return ""
        Try
            chkDate = fldDate.ToString("HH:mm")
        Catch ex As Exception

        End Try
        Return chkDate
    End Function
    Function webTime2(ByVal fldTime As Object) As String
        Dim chkTime As String = ""
        'If fldDate = "null" Then Return ""
        Try
            Dim intervals As TimeSpan
            intervals = CheckDBNull(fldTime)
            chkTime = intervals.ToString
        Catch ex As Exception

        End Try
        Return chkTime
    End Function
    Function webDouble(ByVal fldDate As String) As String
        Dim chkDouble As String = ""
        'If fldDate = "null" Then Return ""
        Try
            chkDouble = CDbl(fldDate)
            Return Replace(chkDouble.ToString, ",", ".")
            'Return chkDouble.ToString(New Globalization.CultureInfo("en-US", False))

        Catch ex As Exception
            Return ""
        End Try
        Return chkDouble
    End Function

    Function webTimeDiff(ByVal dat_polaska As String, ByVal vri_polaska As String, ByVal dat_povratka As String, _
                         ByVal vri_povratka As String) As Double
        Dim t As Double = 0
        Try
            Dim fldDate As String = dat_polaska
            Dim fldTime As String = vri_polaska
            Dim dt1 As DateTime
            dt1 = New DateTime(CInt(fldDate.Substring(6, 4)), CInt(fldDate.Substring(3, 2)), CInt(fldDate.Substring(0, 2)), _
                            fldTime.Substring(0, 2), fldTime.Substring(3, 2), 0)
            Dim dt2 As DateTime
            fldDate = dat_povratka
            fldTime = vri_povratka
            dt2 = New DateTime(CInt(fldDate.Substring(6, 4)), CInt(fldDate.Substring(3, 2)), CInt(fldDate.Substring(0, 2)), _
                    fldTime.Substring(0, 2), fldTime.Substring(3, 2), 0)
            Dim x As TimeSpan
            x = dt2 - dt1
            t = x.TotalHours
        Catch ex As Exception

        End Try

        Return t

    End Function

    Function webCommaToDot(ByVal Parameter As String) As String

        Return Replace(Parameter, ",", ".")

    End Function

    Function webDotToComma(ByVal Parameter As String) As String
        Parameter = Replace(Parameter, ".", ",")
        Dim dParameter As Double = 0
        Try
            dParameter = CDbl(Parameter)
        Catch ex As Exception

        End Try
        Return dParameter.ToString

    End Function

    Function dbTextToDate(ByVal fldDate As String) As Date
        If Trim(fldDate) = "" Then Return Now().Date
        Dim chkDate As Date
        Try
            chkDate = New Date(CInt(fldDate.Substring(6, 4)), _
                                      CInt(fldDate.Substring(3, 2)), _
                                      CInt(fldDate.Substring(0, 2)))
            Return chkDate
        Catch ex As System.Exception
            Return Now().Date
        End Try

    End Function


    Sub setLinks(ByRef link1 As System.Web.UI.HtmlControls.HtmlAnchor, ByRef link2 As System.Web.UI.HtmlControls.HtmlAnchor, _
    ByRef link3 As System.Web.UI.HtmlControls.HtmlAnchor, ByRef link4 As System.Web.UI.HtmlControls.HtmlAnchor, _
    ByRef link5 As System.Web.UI.HtmlControls.HtmlAnchor, ByRef link6 As System.Web.UI.HtmlControls.HtmlAnchor, _
     ByRef link7 As System.Web.UI.HtmlControls.HtmlAnchor, ByVal LoggedIn As Boolean, _
     ByVal strRole As String)

        If LoggedIn Then


            Select Case strRole
                Case "rukovodilac"
                    link1.Visible = True
                    link2.Visible = True
                    link3.Visible = True
                    link4.Visible = True
                    link5.Visible = True
                    link6.Visible = True
                    link7.Visible = True


                    link1.HRef = "administracija_unos.aspx"
                    link1.Target = "_self"
                    link1.Title = "Administracija"
                    link1.InnerText = "Administracija"

                    link2.HRef = "~/Evidencija_GodOdmor.aspx"
                    link2.Target = "_self"
                    link2.Title = "GO Pravna"
                    link2.InnerText = "GO Pravna"

                    link3.HRef = "~/Reports_GodOdm.aspx"
                    link3.Target = "_self"
                    link3.Title = "GO izvještaj"
                    link3.InnerText = "GO izvještaj"

                    link4.HRef = ""
                    link4.Target = "_self"
                    link4.Title = ""
                    link4.InnerText = ""

                    link5.HRef = "~/Evidencija_CalendarZahtjev.aspx"
                    link5.Target = "_self"
                    link5.Title = "Zahtjevi"
                    link5.InnerText = "Zahtjevi"

                    link6.HRef = "~/Evidencija_CalendarC.aspx"
                    link6.Target = "_self"
                    link6.Title = "Evidencije"
                    link6.InnerText = "Evidencije"


                    link7.HRef = "Employees_Unos.aspx"
                    link7.Target = "_self"
                    link7.Title = "Unos uposlenika"
                    link7.InnerText = "Unos uposlenika"
                Case "uposlenik"
                    link1.Visible = True
                    link2.Visible = True
                    link3.Visible = True
                    link4.Visible = True
                    link5.Visible = True
                    link6.Visible = True
                    link7.Visible = True


                    link1.HRef = "administracija_unos.aspx"
                    link1.Target = "_self"
                    link1.Title = "Administracija"
                    link1.InnerText = "Administracija"

                    link2.HRef = "~/Evidencija_GodOdmor.aspx"
                    link2.Target = "_self"
                    link2.Title = "GO Pravna"
                    link2.InnerText = "GO Pravna"

                    link3.HRef = "~/Reports_GodOdm.aspx"
                    link3.Target = "_self"
                    link3.Title = "GO izvještaj"
                    link3.InnerText = "GO izvještaj"

                    link4.HRef = ""
                    link4.Target = "_self"
                    link4.Title = ""
                    link4.InnerText = ""

                    link5.HRef = "~/Evidencija_CalendarZahtjev.aspx"
                    link5.Target = "_self"
                    link5.Title = "Zahtjevi"
                    link5.InnerText = "Zahtjevi"

                    link6.HRef = "~/Evidencija_CalendarC.aspx"
                    link6.Target = "_self"
                    link6.Title = "Evidencije"
                    link6.InnerText = "Evidencije"


                    link7.HRef = "Employees_Unos.aspx"
                    link7.Target = "_self"
                    link7.Title = "Unos uposlenika"
                    link7.InnerText = "Unos uposlenika"
                Case "administrator"
                    link1.Visible = True
                    link2.Visible = True
                    link3.Visible = True
                    link4.Visible = True
                    link5.Visible = True
                    link6.Visible = True
                    link7.Visible = True


                    link1.HRef = "administracija_unos.aspx"
                    link1.Target = "_self"
                    link1.Title = "Administracija"
                    link1.InnerText = "Administracija"

                    link2.HRef = "~/Evidencija_GodOdmor.aspx"
                    link2.Target = "_self"
                    link2.Title = "GO Pravna"
                    link2.InnerText = "GO Pravna"

                    link3.HRef = "~/Reports_GodOdm.aspx"
                    link3.Target = "_self"
                    link3.Title = "GO izvještaj"
                    link3.InnerText = "GO izvještaj"

                    link4.HRef = ""
                    link4.Target = "_self"
                    link4.Title = ""
                    link4.InnerText = ""

                    link5.HRef = "~/Evidencija_CalendarZahtjev.aspx"
                    link5.Target = "_self"
                    link5.Title = "Zahtjevi"
                    link5.InnerText = "Zahtjevi"

                    link6.HRef = "~/Evidencija_CalendarC.aspx"
                    link6.Target = "_self"
                    link6.Title = "Evidencije"
                    link6.InnerText = "Evidencije"


                    link7.HRef = "Employees_Unos.aspx"
                    link7.Target = "_self"
                    link7.Title = "Unos uposlenika"
                    link7.InnerText = "Unos uposlenika"
                Case "blagajna"
                    link1.Visible = True
                    link2.Visible = True
                    link3.Visible = True
                    link4.Visible = True
                    link5.Visible = True
                    link6.Visible = True
                    link7.Visible = True


                    link1.HRef = "administracija_unos.aspx"
                    link1.Target = "_self"
                    link1.Title = "Administracija"
                    link1.InnerText = "Administracija"

                    link2.HRef = "~/Evidencija_GodOdmor.aspx"
                    link2.Target = "_self"
                    link2.Title = "GO Pravna"
                    link2.InnerText = "GO Pravna"

                    link3.HRef = "~/Reports_GodOdm.aspx"
                    link3.Target = "_self"
                    link3.Title = "GO izvještaj"
                    link3.InnerText = "GO izvještaj"

                    link4.HRef = ""
                    link4.Target = "_self"
                    link4.Title = ""
                    link4.InnerText = ""

                    link5.HRef = "~/Evidencija_CalendarZahtjev.aspx"
                    link5.Target = "_self"
                    link5.Title = "Zahtjevi"
                    link5.InnerText = "Zahtjevi"

                    link6.HRef = "~/Evidencija_CalendarC.aspx"
                    link6.Target = "_self"
                    link6.Title = "Evidencije"
                    link6.InnerText = "Evidencije"


                    link7.HRef = "Employees_Unos.aspx"
                    link7.Target = "_self"
                    link7.Title = "Unos uposlenika"
                    link7.InnerText = "Unos uposlenika"
                Case "likvidatura"
                    link1.Visible = True
                    link2.Visible = True
                    link3.Visible = True
                    link4.Visible = True
                    link5.Visible = True
                    link6.Visible = True
                    link7.Visible = True


                    link1.HRef = "administracija_unos.aspx"
                    link1.Target = "_self"
                    link1.Title = "Administracija"
                    link1.InnerText = "Administracija"

                    link2.HRef = "~/Evidencija_GodOdmor.aspx"
                    link2.Target = "_self"
                    link2.Title = "GO Pravna"
                    link2.InnerText = "GO Pravna"

                    link3.HRef = "~/Reports_GodOdm.aspx"
                    link3.Target = "_self"
                    link3.Title = "GO izvještaj"
                    link3.InnerText = "GO izvještaj"

                    link4.HRef = ""
                    link4.Target = "_self"
                    link4.Title = ""
                    link4.InnerText = ""

                    link5.HRef = "~/Evidencija_CalendarZahtjev.aspx"
                    link5.Target = "_self"
                    link5.Title = "Zahtjevi"
                    link5.InnerText = "Zahtjevi"

                    link6.HRef = "~/Evidencija_CalendarC.aspx"
                    link6.Target = "_self"
                    link6.Title = "Evidencije"
                    link6.InnerText = "Evidencije"


                    link7.HRef = "Employees_Unos.aspx"
                    link7.Target = "_self"
                    link7.Title = "Unos uposlenika"
                    link7.InnerText = "Unos uposlenika"
                Case "evidencija"
                    link1.Visible = True
                    link2.Visible = True
                    link3.Visible = True
                    link4.Visible = True
                    link5.Visible = True
                    link6.Visible = True
                    link7.Visible = True


                    link1.HRef = "administracija_unos.aspx"
                    link1.Target = "_self"
                    link1.Title = "Administracija"
                    link1.InnerText = "Administracija"

                    link2.HRef = "~/Evidencija_GodOdmor.aspx"
                    link2.Target = "_self"
                    link2.Title = "GO Pravna"
                    link2.InnerText = "GO Pravna"

                    link3.HRef = "~/Reports_GodOdm.aspx"
                    link3.Target = "_self"
                    link3.Title = "GO izvještaj"
                    link3.InnerText = "GO izvještaj"

                    link4.HRef = ""
                    link4.Target = "_self"
                    link4.Title = ""
                    link4.InnerText = ""

                    link5.HRef = "~/Evidencija_CalendarZahtjev.aspx"
                    link5.Target = "_self"
                    link5.Title = "Zahtjevi"
                    link5.InnerText = "Zahtjevi"

                    link6.HRef = "~/Evidencija_CalendarC.aspx"
                    link6.Target = "_self"
                    link6.Title = "Evidencije"
                    link6.InnerText = "Evidencije"


                    link7.HRef = "Employees_Unos.aspx"
                    link7.Target = "_self"
                    link7.Title = "Unos uposlenika"
                    link7.InnerText = "Unos uposlenika"
                Case "pravna"
                    link1.Visible = True
                    link2.Visible = True
                    link3.Visible = True
                    link4.Visible = True
                    link5.Visible = True
                    link6.Visible = True
                    link7.Visible = True


                    link1.HRef = "administracija_unos.aspx"
                    link1.Target = "_self"
                    link1.Title = "Administracija"
                    link1.InnerText = "Administracija"

                    link2.HRef = "~/Evidencija_GodOdmor.aspx"
                    link2.Target = "_self"
                    link2.Title = "GO Pravna"
                    link2.InnerText = "GO Pravna"

                    link3.HRef = "~/Reports_GodOdm.aspx"
                    link3.Target = "_self"
                    link3.Title = "GO izvještaj"
                    link3.InnerText = "GO izvještaj"

                    link4.HRef = "~/Evidencija_Bolovanja.aspx"
                    link4.Target = "_self"
                    link4.Title = "Unos bolovanja"
                    link4.InnerText = "Unos bolovanja"

                    link5.HRef = "~/Evidencija_CalendarZahtjev.aspx"
                    link5.Target = "_self"
                    link5.Title = "Zahtjevi"
                    link5.InnerText = "Zahtjevi"

                    link6.HRef = "~/Evidencija_CalendarC.aspx"
                    link6.Target = "_self"
                    link6.Title = "Evidencije"
                    link6.InnerText = "Evidencije"


                    link7.HRef = "Employees_Unos.aspx"
                    link7.Target = "_self"
                    link7.Title = "Unos uposlenika"
                    link7.InnerText = "Unos uposlenika"
                Case Else
                    link1.Visible = True
                    link2.Visible = True
                    link3.Visible = True
                    link4.Visible = True
                    link5.Visible = True
                    link6.Visible = True
                    link7.Visible = True


                    link1.HRef = "administracija_unos.aspx"
                    link1.Target = "_self"
                    link1.Title = "Administracija"
                    link1.InnerText = "Administracija"

                    link2.HRef = "~/Evidencija_GodOdmor.aspx"
                    link2.Target = "_self"
                    link2.Title = "GO Pravna"
                    link2.InnerText = "GO Pravna"

                    link3.HRef = "~/Reports_GodOdm.aspx"
                    link3.Target = "_self"
                    link3.Title = "GO izvještaj"
                    link3.InnerText = "GO izvještaj"

                    link4.HRef = ""
                    link4.Target = "_self"
                    link4.Title = ""
                    link4.InnerText = ""

                    link5.HRef = "~/Evidencija_CalendarZahtjev.aspx"
                    link5.Target = "_self"
                    link5.Title = "Zahtjevi"
                    link5.InnerText = "Zahtjevi"

                    link6.HRef = "~/Evidencija_CalendarC.aspx"
                    link6.Target = "_self"
                    link6.Title = "Evidencije"
                    link6.InnerText = "Evidencije"


                    link7.HRef = "evidencija_unos.aspx"
                    link7.Target = "_self"
                    link7.Title = "Zbirna evidencija"
                    link7.InnerText = "Zbirna evidencija"


            End Select



        Else
            link1.Visible = False
            link2.Visible = False
            link3.Visible = False
            link4.Visible = False
            link5.Visible = False
            link6.Visible = False
            link7.Visible = False

        End If


    End Sub

    Sub getEmployeeFullName(ByRef EmployeeName As String, ByRef currentUser As MembershipUser, ByRef EmployeeID As Integer)

        'Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString
        Dim con As New MySqlClient.MySqlConnection(ConnectionString)
        ' TODO : Provjeriti zasto se javlja greska

        If currentUser Is Nothing Then
            EmployeeName = "Sesija neispravna"
            EmployeeID = -1
        End If

        Dim SQL As String = "SELECT my_aspnet_users.id, evd_employees.EmployeeID, evd_employees.LastName, evd_employees.FirstName FROM my_aspnet_users " _
                        & " INNER JOIN my_aspnet_users_to_employees ON (my_aspnet_users.id = my_aspnet_users_to_employees.users_id) " _
                        & "INNER JOIN evd_employees ON (my_aspnet_users_to_employees.employees_id = evd_employees.EmployeeID) " _
                        & " WHERE my_aspnet_users.name='" & currentUser.UserName & "';"


        Dim cmd As New MySqlClient.MySqlCommand(SQL, con)

        con.Open()
        cmd.ExecuteNonQuery()
        Dim rd As MySqlClient.MySqlDataReader = cmd.ExecuteReader()
        Do While rd.Read
            EmployeeName = rd("LastName") & " " & rd("FirstName")
            EmployeeID = rd("EmployeeID")
        Loop
        rd.Close()
        con.Close()

    End Sub


    Public Function DBDateFromDate(ByVal origDate As Date) As String
        ' ----- Prepare a date for insertion in a SQL statement.
        Return "'" & Format(origDate, "yyyy/MM/dd") & "'"
    End Function

    Public Function DBFontStyle(ByVal useFont As System.Drawing.Font) As String
        ' ----- Given a font, extract to a string the possible letters "BIUK"
        '      (bold, italic, underline, strikeout). See the BuildFontStyle
        '      function for the reverse of this function.
        Dim resultStyle As String

        ' ----- Process the string.
        resultStyle = ""
        If (useFont.Bold = True) Then resultStyle &= "B"
        If (useFont.Italic = True) Then resultStyle &= "I"
        If (useFont.Underline = True) Then resultStyle &= "U"
        If (useFont.Strikeout = True) Then resultStyle &= "K"
        Return resultStyle
    End Function

    Public Function DBGetDecimal(ByRef dataField As Object) As Decimal
        ' ----- Return the decimal equivalent of an optional database field.
        If (IsDBNull(dataField) = True) Then
            Return 0@
        Else
            Return CDec(dataField)
        End If
    End Function

    Public Function DBGetInteger(ByRef dataField As Object) As Integer
        ' ----- Return the integer equivalent of an optional database field.
        If (IsDBNull(dataField) = True) Then
            Return 0
        Else
            Return CInt(dataField)
        End If
    End Function

    Public Function DBGetText(ByRef dataField As Object) As String
        ' ----- Return the text equivalent of an optional database field.
        If (IsDBNull(dataField) = True) Then
            Return ""
        Else
            Return CStr(dataField)
        End If
    End Function

    Public Function DBGetBoolean(ByVal dbBoolean As Integer) As Boolean
        ' ----- Return the text equivalent of an optional database field.
        If (IsDBNull(dbBoolean) = True) Then
            Return False
        ElseIf dbBoolean <> 0 Then
            Return True
        Else
            Return False
        End If
    End Function

    Public Function DBNum(ByVal origText As String) As String
        ' ----- Prepare a number for insertion in a SQL statement.
        If (Trim(origText) = "") Then
            Return "NULL"
        Else
            Return Trim(Replace(origText, ",", "."))
        End If
    End Function

    Public Function DBText(ByVal origText As String) As String
        ' ----- Prepare a string for insertion in a SQL statement.
        If (Trim(origText) = "") Then
            'Return "NULL"
            Return "'" & Replace(origText, "'", "''") & "'"
        Else
            Return "'" & Replace(origText, "'", "''") & "'"
        End If
    End Function


    Public Sub GeneralError(ByVal routineName As String, _
            ByVal theError As System.Exception)
        ' ----- Report an error to the user.
        On Error Resume Next

        MsgBox("The following error occurred at location '" & routineName & _
            "':" & vbCrLf & vbCrLf & theError.Message, _
            MsgBoxStyle.OkOnly Or MsgBoxStyle.Exclamation, "RL portal")

    End Sub



End Module
