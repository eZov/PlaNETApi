Imports System.IO
Imports System.Security.Cryptography
Imports Microsoft.VisualBasic
Imports MySql.Data
Imports MySql.Data.MySqlClient
Imports Syncfusion.DocIO
Imports Syncfusion.DocIO.DLS

Public Class ClsEmployeesGo

    Private cFilename As String = ""
    Private cFilesize As UInt32
    Private cRawData() As Byte
    Private fs As FileStream


    Public Sub New(ByRef pEmployee As ClsEmployee, ByRef pEmployeeGo As ClsEmployeeGodOdm)
        Me.Employee = pEmployee
        Me.EmployeeGo = pEmployeeGo
    End Sub

    Public Property Employee As ClsEmployee

    Public Property EmployeeGo As ClsEmployeeGodOdm

    Public Property FilenameCode As String
    Public Property RjesStatus As Integer

    Public Sub odobRjes(ByVal pIdDocStatus As Integer)

        Dim outFileName As String = ""

        Dim mycmd As MySqlCommand
        Dim myrdr As MySqlDataReader

        Dim strSQL As String

        '
        ' Na osnovu id uzima code i status (_N_ i -1)
        '
        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            strSQL = <![CDATA[ 
SELECT eds.`filename_code`, eds.`rjesenje_status` FROM `evd_doc_status` eds WHERE eds.`go_id`=@pIdDocStatus;
]]>.Value
            '
            '   Konverzija iz DateTime u MySqlDateTime !!
            '
            strSQL = strSQL.Replace("@pIdDocStatus", pIdDocStatus)

            mycmd = New MySqlCommand(strSQL, myconnection)
            myrdr = mycmd.ExecuteReader

            While myrdr.Read
                '
                FilenameCode = myrdr.GetString("filename_code")
                RjesStatus = myrdr.GetInt32("rjesenje_status")
            End While


        End Using

        '
        '   Kreira code za redove uvezane sa prethodnim (_N1_ ili _N2_), zavisno da li je GO
        '   u 1 terminu ili 2 termina
        '
        Dim Result As String = Regex.Replace(FilenameCode, "[^a-zA-Z]", "")

        If EmployeeGo.plan1Dani > 0 And EmployeeGo.plan2Dani = 0 Then
            FilenameCode = String.Format("_{0}1_", Result)
        ElseIf EmployeeGo.plan1Dani > 0 And EmployeeGo.plan2Dani > 0 Then
            FilenameCode = String.Format("_{0}2_", Result)
        Else
            Exit Sub
        End If

        '
        ' Uzima M i F templejte za 
        '
        Select Case Employee.Pol
            Case "F"
                strSQL = <![CDATA[ 
SELECT   edt.`filename`, edt.`filesize`, edt.`filebody`   FROM `evd_doc_template` edt, `evd_doc_status` eds  
WHERE eds.`filename_code` LIKE ('@FilenameCode') AND eds.`rjesenje_status`=0
AND edt.`md5`=eds.`template_md5_f`;
]]>.Value
            Case "M"
                strSQL = <![CDATA[ 
SELECT   edt.`filename`, edt.`filesize`, edt.`filebody`   FROM `evd_doc_template` edt, `evd_doc_status` eds  
WHERE eds.`filename_code` LIKE ('@FilenameCode') AND eds.`rjesenje_status`=0
AND edt.`md5`=eds.`template_md5_m`;
]]>.Value
        End Select

        strSQL = strSQL.Replace("@FilenameCode", FilenameCode)


        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            mycmd = New MySqlCommand(strSQL, myconnection)
            myrdr = mycmd.ExecuteReader

            While myrdr.Read
                '
                cFilename = myrdr.GetString("filename")
                cFilesize = myrdr.GetUInt32("filesize")

                cRawData = New Byte(cFilesize) {}

                myrdr.GetBytes(myrdr.GetOrdinal("filebody"), 0, cRawData, 0, cFilesize)

            End While

        End Using

        '
        '   Spašava templejt na IIS folder
        '
        If Not File.Exists(HttpContext.Current.Server.MapPath("~/Data/" + cFilename)) Then
            fs = New FileStream(HttpContext.Current.Server.MapPath("~/Data/" + cFilename), FileMode.Create, FileAccess.ReadWrite)
            fs.Write(cRawData, 0, cFilesize)
            fs.Close()
        End If


        '
        '   Procesira templejt u rješenje
        '
        If File.Exists(HttpContext.Current.Server.MapPath("~/Data/" + cFilename)) Then
            '   Load an existing Word document
            Dim document As WordDocument = New WordDocument(HttpContext.Current.Server.MapPath("~/Data/" + cFilename))
            Dim paragraph As WParagraph = document.Sections(0).Paragraphs(1)


            Dim selection As TextSelection = document.Find(New Regex("%IME_PREZIME%,"))


            document.Replace(New Regex("%DATUM_RJES%"), EmployeeGo.rjDatRjes.ToString("dd.MM.yyyy"))

            document.Replace(New Regex("%IME_PREZIME%"), Employee.FirstName + " " + Employee.LastName)
            document.Replace(New Regex("%JMB%"), Employee.JMB)
            document.Replace(New Regex("%RADNO_MJESTO% "), Employee.RadMjesXfr)
            document.Replace(New Regex("%ORG_JED%"), Employee.OrgJedUpoXfr)


            document.Replace(New Regex("%TRAJANJE_GO%"), EmployeeGo.DaniGO)
            document.Replace(New Regex("%GODINA%"), EmployeeGo.Godina)

            document.Replace(New Regex("%DIO11%"), EmployeeGo.plan1StartDate)
            document.Replace(New Regex("%DIO12%"), EmployeeGo.plan1EndDate)
            document.Replace(New Regex("%DIO13%"), EmployeeGo.plan1RadDate)
            document.Replace(New Regex("%DIO1_DANA%"), EmployeeGo.plan1Dani)

            document.Replace(New Regex("%DIO21%"), EmployeeGo.plan2StartDate)
            document.Replace(New Regex("%DIO22%"), EmployeeGo.plan2EndDate)
            document.Replace(New Regex("%DIO23%"), EmployeeGo.plan2RadDate)
            document.Replace(New Regex("%DIO2_DANA%"), EmployeeGo.plan2Dani)

            document.Replace(New Regex("%DIREKTOR%"), EmployeeGo.rjPotpisnik)
            document.Replace(New Regex("%PROT1%"), EmployeeGo.rjProt1)
            document.Replace(New Regex("%PROT2%"), EmployeeGo.rjProt2)

            'document.Replace(New Regex("%VANSNAGERB%"), pVanSnage)
            '   Save And Close the document

            outFileName = String.Format("GO_rjesenje-{0}_{1}{2}.docx", Employee.LastName + "_" + Employee.EmployeeId.ToString + "_", DateTime.Now.ToString("yyyy"), FilenameCode)


            document.Save(HttpContext.Current.Server.MapPath("~/Data/" + outFileName), FormatType.Docx)
            document.Close()

        End If

        '
        '   Kreira MD5 rješenja
        '
        Dim strMD5 As String = ""

        Dim outFilePath As String = HttpContext.Current.Server.MapPath("~/Data/" + outFileName)
        fs = New FileStream(outFilePath, FileMode.Open, FileAccess.Read)
        cFilesize = fs.Length

        cRawData = New Byte(cFilesize) {}
        fs.Read(cRawData, 0, cFilesize)
        fs.Close()

        strMD5 = ClsFiles.CreateMD5(outFilePath)


        '
        '   Upisuje fajl rješenja i MD5 u bazu
        '
        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            strSQL = <![CDATA[ 
INSERT INTO evd_doc_documents VALUES (NULL,  @file_md5, @file_name, @file_size, @file_body, NOW() );
]]>.Value

            mycmd = New MySqlCommand(strSQL, myconnection)

            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@file_md5", strMD5)
            mycmd.Parameters.AddWithValue("@file_name", outFileName)
            mycmd.Parameters.AddWithValue("@file_size", cFilesize)
            mycmd.Parameters.AddWithValue("@file_body", cRawData)

            mycmd.ExecuteNonQuery()

        End Using

        '
        '   Mijenja status i upisuje u arhivu
        '
        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            strSQL = <![CDATA[ 
CALL GO_PostaviRjesenjaEmp(@pVrstaRjes,  @pEmployee, @pGodina, @pMD5, @pDatRjes, @pPotpisnik)
]]>.Value

            mycmd = New MySqlCommand(strSQL, myconnection)

            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@pVrstaRjes", RjesStatus)
            mycmd.Parameters.AddWithValue("@pEmployee", Employee.EmployeeId)
            mycmd.Parameters.AddWithValue("@pGodina", EmployeeGo.Godina)
            mycmd.Parameters.AddWithValue("@pMD5", strMD5)
            mycmd.Parameters.AddWithValue("@pDatRjes", EmployeeGo.rjDatRjes.ToString("yyyy-MM-dd"))
            mycmd.Parameters.AddWithValue("@pPotpisnik", EmployeeGo.rjPotpisnik)

            mycmd.ExecuteNonQuery()

        End Using
    End Sub

    Public Function CreateMD5(ByVal pFilePathName As String) As String

        Try
            Dim RD As FileStream
            RD = New FileStream(pFilePathName, FileMode.Open, FileAccess.Read, FileShare.Read, 8192)
            Dim md5 As MD5CryptoServiceProvider = New MD5CryptoServiceProvider
            md5.ComputeHash(RD)
            RD.Close()

            'converting the bytes into string
            Dim hash As Byte() = md5.Hash
            Dim SB As StringBuilder = New StringBuilder
            Dim HB As Byte
            For Each HB In hash
                SB.Append(String.Format("{0:X2}", HB))
            Next
            Return SB.ToString()


        Catch ex As Exception

        End Try

        Return Nothing

    End Function
End Class
