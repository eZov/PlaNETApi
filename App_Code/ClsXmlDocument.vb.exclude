Imports Microsoft.VisualBasic
Imports System.Reflection
Imports System
Imports MySql.Data
Imports System.Xml
Imports MySql.Data.MySqlClient

Public Class ClsXmlDocument

    Private cXmldoc As XmlDocument
    Private cXmlRoot As XmlNode
    Private cFileLocation As String
    Private cObracun As Integer
    Private sMjesec As Integer
    Private sGodina As Integer


    Public Sub New()
        cXmldoc = New XmlDocument
    End Sub

    Public ReadOnly Property XmlRoot() As XmlNode
        Get
            Return cXmlRoot
        End Get
    End Property

    Public Property FileLocation() As String
        Get
            Return cFileLocation
        End Get
        Set(ByVal value As String)
            cFileLocation = value
        End Set
    End Property
    Public Property Obracun() As Integer
        Get
            Return cObracun
        End Get
        Set(ByVal value As Integer)
            cObracun = value
        End Set
    End Property

    Public Sub CreateElement(ByRef rXmlNode As XmlNode, ByVal sXmlNodeName As String, Optional ByVal sXmlNodeText As String = "")
        Dim sXmlNode As XmlNode
        sXmlNode = cXmldoc.CreateElement(sXmlNodeName)
        sXmlNode.InnerText = sXmlNodeText
        rXmlNode.AppendChild(sXmlNode)
    End Sub

    Public Function CreateElementAndReturnChild(ByRef rXmlNode As XmlNode, ByVal sXmlNodeName As String, Optional ByVal sXmlNodeText As String = "") As XmlNode
        Try
            Dim sXmlNode As XmlNode
            sXmlNode = cXmldoc.CreateElement(sXmlNodeName)
            sXmlNode.InnerText = sXmlNodeText
            rXmlNode.AppendChild(sXmlNode)
            Return sXmlNode
        Catch ex As Exception
            Dim el As New ErrorsAndEvents.ErrorLogger
            el.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
            'MsgBox("Error logged.")
            Return Nothing
        End Try

    End Function

    Public Function FindNodeByName(ByVal rXmlNodeName As String) As XmlNode
        Try
            Dim nodeList As XmlNodeList = cXmldoc.GetElementsByTagName(rXmlNodeName)

            Return nodeList.Item(0)


        Catch ex As Exception
            Dim el As New ErrorsAndEvents.ErrorLogger
            el.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
            'MsgBox("Error logged.")
            Return Nothing
        End Try
    End Function

    Public Sub SaveXmlFile(ByVal sXmlFileName As String)
        cXmldoc.Save(sXmlFileName)
    End Sub

    Public Sub CreateStructure_MIP1023()
        cXmlRoot = cXmldoc.CreateElement("PaketniUvozObrazaca")
        cXmldoc.AppendChild(cXmlRoot)

        ' Create an attribute and add it to the root element
        cXmldoc.DocumentElement.SetAttribute("xmlns", "urn:PaketniUvozObrazaca_V1_0.xsd")




        CreateElement(cXmlRoot, "PodaciOPoslodavcu")
        CreateElement(cXmlRoot, "Obrazac1023")




        CreateElement(FindNodeByName("Obrazac1023"), "Dio1")
        CreateElement(FindNodeByName("Obrazac1023"), "Dio2")
        CreateElement(FindNodeByName("Obrazac1023"), "Dio3")
        CreateElement(FindNodeByName("Obrazac1023"), "Dio4IzjavaPoslodavca")
        CreateElement(FindNodeByName("Obrazac1023"), "Dokument")





    End Sub

    Public Overloads Sub CreateMIP1023_PodaciOPoslodavcu(ByRef sParameters As Hashtable)
        CreateElement(FindNodeByName("PodaciOPoslodavcu"), "JIBPoslodavca", sParameters("d4_JibJmbPoslodavca"))
        CreateElement(FindNodeByName("PodaciOPoslodavcu"), "NazivPoslodavca", sParameters("d4_NazivPoslodavca"))
        CreateElement(FindNodeByName("PodaciOPoslodavcu"), "BrojZahtjeva", sParameters("BrojZahtjeva"))
        CreateElement(FindNodeByName("PodaciOPoslodavcu"), "DatumPodnosenja", sParameters("d4_DatumUnosa"))
    End Sub

    Public Overloads Sub CreateMIP1023_PodaciOPoslodavcu()
        CreateElement(FindNodeByName("PodaciOPoslodavcu"), "JIBPoslodavca", "1111111111111")
        CreateElement(FindNodeByName("PodaciOPoslodavcu"), "NazivPoslodavca", "TEST")
        CreateElement(FindNodeByName("PodaciOPoslodavcu"), "BrojZahtjeva", "999")
        CreateElement(FindNodeByName("PodaciOPoslodavcu"), "DatumPodnosenja", "2011-11-12")
    End Sub

    Public Overloads Sub CreateMIP1023_Dio1(ByRef sParameters As Hashtable)
        CreateElement(FindNodeByName("Dio1"), "JibJmb", sParameters("d1_JibJmb"))
        CreateElement(FindNodeByName("Dio1"), "Naziv", sParameters("d1_Naziv"))
        CreateElement(FindNodeByName("Dio1"), "DatumUpisa", sParameters("d1_DatumUpisa"))
        CreateElement(FindNodeByName("Dio1"), "BrojUposlenih", sParameters("d1_BrojUposlenih"))
        CreateElement(FindNodeByName("Dio1"), "PeriodOd", sParameters("d1_PeriodOd"))
        CreateElement(FindNodeByName("Dio1"), "PeriodDo", sParameters("d1_PeriodDo"))
        CreateElement(FindNodeByName("Dio1"), "SifraDjelatnosti", sParameters("d1_SifraDjelatnosti"))
    End Sub

    Public Overloads Sub CreateMIP1023_Dio1()
        CreateElement(FindNodeByName("Dio1"), "JibJmb")
        CreateElement(FindNodeByName("Dio1"), "Naziv")
        CreateElement(FindNodeByName("Dio1"), "SifraDjelatnosti")
        CreateElement(FindNodeByName("Dio1"), "BrojUposlenih")
        CreateElement(FindNodeByName("Dio1"), "DatumUpisa")
        CreateElement(FindNodeByName("Dio1"), "PeriodOd")
        CreateElement(FindNodeByName("Dio1"), "PeriodDo")
    End Sub

    Public Overloads Sub CreateMIP1023_Dio3(ByRef sParameters As Hashtable)
        CreateElement(FindNodeByName("Dio3"), "PIO", sParameters("d3_PIO"))
        CreateElement(FindNodeByName("Dio3"), "ZO", sParameters("d3_ZO"))
        CreateElement(FindNodeByName("Dio3"), "OsiguranjeOdNezaposlenosti", sParameters("d3_OsiguranjeOdNezaposlenosti"))
        CreateElement(FindNodeByName("Dio3"), "DodatniDoprinosiZO", sParameters("d3_DodatniDoprinosiZO"))
        CreateElement(FindNodeByName("Dio3"), "Prihod", sParameters("d3_Prihod"))
        CreateElement(FindNodeByName("Dio3"), "Doprinosi", sParameters("d3_Doprinosi"))
        CreateElement(FindNodeByName("Dio3"), "LicniOdbici", sParameters("d3_LicniOdbici"))
        CreateElement(FindNodeByName("Dio3"), "Porez", sParameters("d3_Porez"))
    End Sub

    Public Overloads Sub CreateMIP1023_Dio3()
        CreateElement(FindNodeByName("Dio3"), "PIO")
        CreateElement(FindNodeByName("Dio3"), "ZO")
        CreateElement(FindNodeByName("Dio3"), "DodatniDoprinosiZO")
        CreateElement(FindNodeByName("Dio3"), "Prihod")
        CreateElement(FindNodeByName("Dio3"), "Doprinosi")
        CreateElement(FindNodeByName("Dio3"), "LicniOdbici")
        CreateElement(FindNodeByName("Dio3"), "Porez")
    End Sub


    Public Overloads Sub CreateMIP1023_Dio4IzjavaPoslodavca(ByRef sParameters As Hashtable)

        CreateElement(FindNodeByName("Dio4IzjavaPoslodavca"), "JibJmbPoslodavca", sParameters("JIBPoslodavca"))
        CreateElement(FindNodeByName("Dio4IzjavaPoslodavca"), "DatumUnosa", sParameters("d4_DatumUnosa"))
        CreateElement(FindNodeByName("Dio4IzjavaPoslodavca"), "NazivPoslodavca", sParameters("NazivPoslodavca"))
    End Sub

    Public Overloads Sub CreateMIP1023_Dio4IzjavaPoslodavca()

        CreateElement(FindNodeByName("Dio4IzjavaPoslodavca"), "JibJmbPoslodavca")
        CreateElement(FindNodeByName("Dio4IzjavaPoslodavca"), "DatumUnosa")
        CreateElement(FindNodeByName("Dio4IzjavaPoslodavca"), "NazivPoslodavca")
    End Sub

    Public Overloads Sub CreateMIP1023_Dokument(ByRef sParameters As Hashtable)

        CreateElement(FindNodeByName("Dokument"), "Operacija", sParameters("NazivDokumenta"))

    End Sub

    Public Overloads Sub CreateMIP1023_Dokument()

        CreateElement(FindNodeByName("Dokument"), "Operacija")

    End Sub

    Public Overloads Sub CreateMIP1023_PodaciOPrihodima(ByRef sParameters As Hashtable)

        Dim nodeDio2 As XmlNode = FindNodeByName("Dio2")

        Dim nodePodaciOPrihodima As XmlNode = CreateElementAndReturnChild(nodeDio2, "PodaciOPrihodima")

        CreateElement(nodePodaciOPrihodima, "VrstaIsplate", sParameters("VrstaIsplate"))
        CreateElement(nodePodaciOPrihodima, "Jmb", sParameters("Jmb"))
        CreateElement(nodePodaciOPrihodima, "ImePrezime", sParameters("ImePrezime"))
        CreateElement(nodePodaciOPrihodima, "DatumIsplate", sParameters("DatumIsplate"))
        CreateElement(nodePodaciOPrihodima, "RadniSati", sParameters("RadniSati"))
        CreateElement(nodePodaciOPrihodima, "RadniSatiBolovanje", sParameters("RadniSatiBolovanje"))
        CreateElement(nodePodaciOPrihodima, "BrutoPlaca", sParameters("BrutoPlaca"))
        CreateElement(nodePodaciOPrihodima, "KoristiIDrugiOporeziviPrihodi", sParameters("KoristiIDrugiOporeziviPrihodi"))
        CreateElement(nodePodaciOPrihodima, "UkupanPrihod", sParameters("UkupanPrihod"))
        CreateElement(nodePodaciOPrihodima, "IznosPIO", sParameters("IznosPIO"))
        CreateElement(nodePodaciOPrihodima, "IznosZO", sParameters("IznosZO"))
        CreateElement(nodePodaciOPrihodima, "IznosNezaposlenost", sParameters("IznosNezaposlenost"))
        CreateElement(nodePodaciOPrihodima, "Doprinosi", sParameters("Doprinosi"))
        CreateElement(nodePodaciOPrihodima, "PrihodUmanjenZaDoprinose", sParameters("PrihodUmanjenZaDoprinose"))
        CreateElement(nodePodaciOPrihodima, "FaktorLicnogOdbitka", sParameters("FaktorLicnogOdbitka"))
        CreateElement(nodePodaciOPrihodima, "IznosLicnogOdbitka", sParameters("IznosLicnogOdbitka"))
        CreateElement(nodePodaciOPrihodima, "OsnovicaPoreza", sParameters("OsnovicaPoreza"))
        CreateElement(nodePodaciOPrihodima, "IznosPoreza", sParameters("IznosPoreza"))
        CreateElement(nodePodaciOPrihodima, "RadniSatiUT", sParameters("RadniSatiUT"))
        CreateElement(nodePodaciOPrihodima, "StepenUvecanja", sParameters("StepenUvecanja"))
        CreateElement(nodePodaciOPrihodima, "SifraRadnogMjestaUT", sParameters("SifraRadnogMjestaUT"))
        CreateElement(nodePodaciOPrihodima, "DoprinosiPIOMIOzaUT", sParameters("DoprinosiPIOMIOzaUT"))
        CreateElement(nodePodaciOPrihodima, "BeneficiraniStaz", sParameters("BeneficiraniStaz"))
        CreateElement(nodePodaciOPrihodima, "OpcinaPrebivalista", sParameters("OpcinaPrebivalista"))

    End Sub

    Public Overloads Sub CreateMIP1023_PodaciOPrihodima()

        Dim nodeDio2 As XmlNode = FindNodeByName("Dio2")

        Dim nodePodaciOPrihodima As XmlNode = CreateElementAndReturnChild(nodeDio2, "PodaciOPrihodima")

        CreateElement(nodePodaciOPrihodima, "VrstaIsplate")
        CreateElement(nodePodaciOPrihodima, "Jmb")
        CreateElement(nodePodaciOPrihodima, "ImePrezime")
        CreateElement(nodePodaciOPrihodima, "DatumIsplate")
        CreateElement(nodePodaciOPrihodima, "RadniSati")
        CreateElement(nodePodaciOPrihodima, "RadniSatiBolovanje")
        CreateElement(nodePodaciOPrihodima, "BrutoPlaca")
        CreateElement(nodePodaciOPrihodima, "KoristiIDrugiOporeziviPrihodi")
        CreateElement(nodePodaciOPrihodima, "UkupanPrihod")
        CreateElement(nodePodaciOPrihodima, "IznosPIO")
        CreateElement(nodePodaciOPrihodima, "IznosZO")
        CreateElement(nodePodaciOPrihodima, "IznosNezaposlenost")
        CreateElement(nodePodaciOPrihodima, "Doprinosi")
        CreateElement(nodePodaciOPrihodima, "PrihodUmanjenZaDoprinose")
        CreateElement(nodePodaciOPrihodima, "FaktorLicnogOdbitka")
        CreateElement(nodePodaciOPrihodima, "IznosLicnogOdbitka")
        CreateElement(nodePodaciOPrihodima, "OsnovicaPoreza")
        CreateElement(nodePodaciOPrihodima, "IznosPoreza")
        CreateElement(nodePodaciOPrihodima, "RadniSatiUT")
        CreateElement(nodePodaciOPrihodima, "StepenUvecanja")
        CreateElement(nodePodaciOPrihodima, "SifraRadnogMjestaUT")
        CreateElement(nodePodaciOPrihodima, "DoprinosiPIOMIOzaUT")
        CreateElement(nodePodaciOPrihodima, "OpcinaPrebivalista")
        CreateElement(nodePodaciOPrihodima, "BeneficiraniStaz")
    End Sub

    Public Sub FillMIP1023()
        Dim sReader As MySql.Data.MySqlClient.MySqlDataReader
        Dim sd1_Parameters As New Hashtable
        Dim sd4_Parameters As New Hashtable
        Dim sd3_Parameters As New Hashtable
        Dim sdDokument_Parameters As New Hashtable
        Dim sdPodaciOPrihodima_Parameters As New Hashtable

        Dim mycmd As MySqlCommand

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            mycmd = New MySqlCommand
            mycmd.Connection = myconnection


            Dim strSQL As String = <![CDATA[ 
CALL rpt_prz_mip1023_temp(@idObracun,'d1')
]]>.Value

            mycmd.CommandText = strSQL


            mycmd.Parameters.AddWithValue("@idObracun", cObracun)
            mycmd.Prepare()

            sReader = mycmd.ExecuteReader()
            While sReader.Read
                Try
                    sd1_Parameters.Add("d1_JibJmb", sReader("d1_JibJmb"))
                    sd1_Parameters.Add("d1_Naziv", sReader("d1_Naziv"))
                    sd1_Parameters.Add("d1_SifraDjelatnosti", sReader("d1_SifraDjelatnosti"))
                    sd1_Parameters.Add("d1_BrojUposlenih", sReader("d1_BrojUposlenih"))
                    sd1_Parameters.Add("d1_DatumUpisa", sReader("d1_DatumUpisa"))
                    sd1_Parameters.Add("d1_PeriodOd", sReader("d1_PeriodOd"))
                    sd1_Parameters.Add("d1_PeriodDo", sReader("d1_PeriodDo"))
                Catch ex As Exception
                    Dim el As New ErrorsAndEvents.ErrorLogger
                    el.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
                End Try

            End While
            sReader.Close()

        End Using


        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            mycmd = New MySqlCommand
            mycmd.Connection = myconnection


            Dim strSQL As String = <![CDATA[ 
CALL rpt_prz_mip1023_temp(@idObracun,'d4')
]]>.Value

            mycmd.CommandText = strSQL


            mycmd.Parameters.AddWithValue("@idObracun", cObracun)
            mycmd.Prepare()

            sReader = mycmd.ExecuteReader()
            While sReader.Read
                Try
                    sd4_Parameters.Add("d4_JibJmbPoslodavca", sReader("d4_JibJmbPoslodavca"))
                    sd4_Parameters.Add("d4_DatumUnosa", sReader("d4_DatumUnosa"))
                    sd4_Parameters.Add("d4_NazivPoslodavca", sReader("d4_NazivPoslodavca"))
                    sd4_Parameters.Add("JIBPoslodavca", sReader("JIBPoslodavca"))
                    sd4_Parameters.Add("NazivPoslodavca", sReader("NazivPoslodavca"))
                    sd4_Parameters.Add("BrojZahtjeva", sReader("BrojZahtjeva"))
                    sd4_Parameters.Add("DatumPodnosenja", sReader("d4_DatumUnosa"))


                Catch ex As Exception
                    Dim el As New ErrorsAndEvents.ErrorLogger
                    el.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
                End Try

            End While
            sReader.Close()

        End Using


        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            mycmd = New MySqlCommand
            mycmd.Connection = myconnection


            Dim strSQL As String = <![CDATA[ 
CALL rpt_prz_mip1023_temp(@idObracun,'d3')
]]>.Value

            mycmd.CommandText = strSQL


            mycmd.Parameters.AddWithValue("@idObracun", cObracun)
            mycmd.Prepare()

            sReader = mycmd.ExecuteReader()
            While sReader.Read
                Try
                    sd3_Parameters.Add("d3_PIO", FormatCommaToPoint(sReader("d3_PIO")))
                    sd3_Parameters.Add("d3_ZO", FormatCommaToPoint(sReader("d3_ZO")))
                    sd3_Parameters.Add("d3_OsiguranjeOdNezaposlenosti", FormatCommaToPoint(sReader("d3_OsiguranjeOdNezaposlenosti")))
                    sd3_Parameters.Add("d3_DodatniDoprinosiZO", FormatCommaToPoint(sReader("d3_DodatniDoprinosiZO")))
                    sd3_Parameters.Add("d3_Prihod", FormatCommaToPoint(sReader("d3_Prihod")))
                    sd3_Parameters.Add("d3_Doprinosi", FormatCommaToPoint(sReader("d3_Doprinosi")))
                    sd3_Parameters.Add("d3_LicniOdbici", FormatCommaToPoint(sReader("d3_LicniOdbici")))
                    sd3_Parameters.Add("d3_Porez", FormatCommaToPoint(sReader("d3_Porez")))
                Catch ex As Exception
                    Dim el As New ErrorsAndEvents.ErrorLogger
                    el.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
                End Try

            End While
            sReader.Close()

        End Using

        sdDokument_Parameters.Add("NazivDokumenta", "Prijava_od_strane_poreznog_obveznika")

        CreateStructure_MIP1023()
        CreateMIP1023_PodaciOPoslodavcu(sd4_Parameters)
        CreateMIP1023_Dio1(sd1_Parameters)
        CreateMIP1023_Dio3(sd3_Parameters)
        CreateMIP1023_Dio4IzjavaPoslodavca(sd4_Parameters)
        CreateMIP1023_Dokument(sdDokument_Parameters)



        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            mycmd = New MySqlCommand
            mycmd.Connection = myconnection


            Dim strSQL As String = <![CDATA[ 
CALL rpt_prz_mip1023_temp(@idObracun,'dp')
]]>.Value

            mycmd.CommandText = strSQL


            mycmd.Parameters.AddWithValue("@idObracun", cObracun)
            mycmd.Prepare()

            sReader = mycmd.ExecuteReader()
            While sReader.Read
                Try
                    sdPodaciOPrihodima_Parameters.Clear()
                    sdPodaciOPrihodima_Parameters.Add("VrstaIsplate", sReader("VrstaIsplate"))
                    sdPodaciOPrihodima_Parameters.Add("Jmb", sReader("Jmb"))
                    sdPodaciOPrihodima_Parameters.Add("ImePrezime", sReader("ImePrezime"))
                    sdPodaciOPrihodima_Parameters.Add("DatumIsplate", sReader("DatumIsplate"))
                    sdPodaciOPrihodima_Parameters.Add("RadniSati", FormatCommaToPoint(sReader("RadniSati")))
                    sdPodaciOPrihodima_Parameters.Add("RadniSatiBolovanje", FormatCommaToPoint(sReader("RadniSatiBolovanje")))
                    sdPodaciOPrihodima_Parameters.Add("BrutoPlaca", FormatCommaToPoint(sReader("BrutoPlaca")))
                    sdPodaciOPrihodima_Parameters.Add("KoristiIDrugiOporeziviPrihodi", FormatCommaToPoint(sReader("KoristiIDrugiOporeziviPrihodi")))
                    sdPodaciOPrihodima_Parameters.Add("UkupanPrihod", FormatCommaToPoint(sReader("UkupanPrihod")))
                    sdPodaciOPrihodima_Parameters.Add("IznosPIO", FormatCommaToPoint(sReader("IznosPIO")))
                    sdPodaciOPrihodima_Parameters.Add("IznosZO", FormatCommaToPoint(sReader("IznosZO")))
                    sdPodaciOPrihodima_Parameters.Add("IznosNezaposlenost", FormatCommaToPoint(sReader("IznosNezaposlenost")))
                    sdPodaciOPrihodima_Parameters.Add("Doprinosi", FormatCommaToPoint(sReader("Doprinosi")))
                    sdPodaciOPrihodima_Parameters.Add("PrihodUmanjenZaDoprinose", FormatCommaToPoint(sReader("PrihodUmanjenZaDoprinose")))
                    sdPodaciOPrihodima_Parameters.Add("FaktorLicnogOdbitka", FormatCommaToPoint(sReader("FaktorLicnogOdbitka")))
                    sdPodaciOPrihodima_Parameters.Add("IznosLicnogOdbitka", FormatCommaToPoint(sReader("IznosLicnogOdbitka")))
                    sdPodaciOPrihodima_Parameters.Add("OsnovicaPoreza", FormatCommaToPoint(sReader("OsnovicaPoreza")))
                    sdPodaciOPrihodima_Parameters.Add("IznosPoreza", FormatCommaToPoint(sReader("IznosPoreza")))
                    sdPodaciOPrihodima_Parameters.Add("RadniSatiUT", FormatCommaToPoint(sReader("RadniSatiUT")))
                    sdPodaciOPrihodima_Parameters.Add("StepenUvecanja", FormatDoubleToInteger(sReader("StepenUvecanja")))
                    sdPodaciOPrihodima_Parameters.Add("SifraRadnogMjestaUT", sReader("SifraRadnogMjestaUT"))
                    sdPodaciOPrihodima_Parameters.Add("DoprinosiPIOMIOzaUT", FormatCommaToPoint(sReader("DoprinosiPIOMIOzaUT")))
                    sdPodaciOPrihodima_Parameters.Add("OpcinaPrebivalista", sReader("OpcinaPrebivalista"))
                    sdPodaciOPrihodima_Parameters.Add("BeneficiraniStaz", ConvertStringToLower(sReader("BeneficiraniStaz")))
                    CreateMIP1023_PodaciOPrihodima(sdPodaciOPrihodima_Parameters)
                Catch ex As Exception
                    Dim el As New ErrorsAndEvents.ErrorLogger
                    el.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
                End Try

            End While
            sReader.Close()

        End Using

        Dim sMjeGodObracuna As String = CType(sd1_Parameters("d1_PeriodOd"), String)
        sMjeGodObracuna = "_" + sMjeGodObracuna.Substring(5, 2) + sMjeGodObracuna.Substring(0, 4)
        SaveXmlFile(cFileLocation + "\" + sd4_Parameters("d4_JibJmbPoslodavca") + sMjeGodObracuna + ".xml")
    End Sub

    Public Sub FillMIP1023PoMjes(ByVal pMjesec As Integer, ByVal pGodina As Integer)
        Dim sReader As MySql.Data.MySqlClient.MySqlDataReader
        Dim sd1_Parameters As New Hashtable
        Dim sd4_Parameters As New Hashtable
        Dim sd3_Parameters As New Hashtable
        Dim sdDokument_Parameters As New Hashtable
        Dim sdPodaciOPrihodima_Parameters As New Hashtable

        Dim mycmd As MySqlCommand

        Me.sMjesec = pMjesec
        Me.sGodina = pGodina

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            mycmd = New MySqlCommand
            mycmd.Connection = myconnection


            Dim strSQL As String = <![CDATA[ 
CALL rpt_prz_mip1023_mjes('d1',@pGodina,@pMjesec)
]]>.Value

            mycmd.CommandText = strSQL


            mycmd.Parameters.AddWithValue("@pGodina", sGodina)
            mycmd.Parameters.AddWithValue("@pMjesec", sMjesec)
            mycmd.Prepare()

            sReader = mycmd.ExecuteReader()
            While sReader.Read
                Try
                    sd1_Parameters.Add("d1_JibJmb", sReader("d1_JibJmb"))
                    sd1_Parameters.Add("d1_Naziv", sReader("d1_Naziv"))
                    sd1_Parameters.Add("d1_SifraDjelatnosti", sReader("d1_SifraDjelatnosti"))
                    sd1_Parameters.Add("d1_BrojUposlenih", sReader("d1_BrojUposlenih"))
                    sd1_Parameters.Add("d1_DatumUpisa", sReader("d1_DatumUpisa"))
                    sd1_Parameters.Add("d1_PeriodOd", sReader("d1_PeriodOd"))
                    sd1_Parameters.Add("d1_PeriodDo", sReader("d1_PeriodDo"))
                Catch ex As Exception
                    Dim el As New ErrorsAndEvents.ErrorLogger
                    el.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
                End Try

            End While
            sReader.Close()

        End Using



        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            mycmd = New MySqlCommand
            mycmd.Connection = myconnection


            Dim strSQL As String = <![CDATA[ 
CALL rpt_prz_mip1023_mjes('d4',@pGodina,@pMjesec)
]]>.Value

            mycmd.CommandText = strSQL


            mycmd.Parameters.AddWithValue("@pGodina", sGodina)
            mycmd.Parameters.AddWithValue("@pMjesec", sMjesec)
            mycmd.Prepare()

            sReader = mycmd.ExecuteReader()
            While sReader.Read
                Try
                    sd4_Parameters.Add("d4_JibJmbPoslodavca", sReader("d4_JibJmbPoslodavca"))
                    sd4_Parameters.Add("d4_DatumUnosa", sReader("d4_DatumUnosa"))
                    sd4_Parameters.Add("d4_NazivPoslodavca", sReader("d4_NazivPoslodavca"))
                    sd4_Parameters.Add("JIBPoslodavca", sReader("JIBPoslodavca"))
                    sd4_Parameters.Add("NazivPoslodavca", sReader("NazivPoslodavca"))
                    sd4_Parameters.Add("BrojZahtjeva", sReader("BrojZahtjeva"))
                    sd4_Parameters.Add("DatumPodnosenja", sReader("d4_DatumUnosa"))
                Catch ex As Exception
                    Dim el As New ErrorsAndEvents.ErrorLogger
                    el.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
                End Try

            End While
            sReader.Close()

        End Using



        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            mycmd = New MySqlCommand
            mycmd.Connection = myconnection


            Dim strSQL As String = <![CDATA[ 
CALL rpt_prz_mip1023_mjes('d3',@pGodina,@pMjesec)
]]>.Value

            mycmd.CommandText = strSQL


            mycmd.Parameters.AddWithValue("@pGodina", sGodina)
            mycmd.Parameters.AddWithValue("@pMjesec", sMjesec)
            mycmd.Prepare()

            sReader = mycmd.ExecuteReader()
            While sReader.Read
                Try
                    sd3_Parameters.Add("d3_PIO", FormatCommaToPoint(sReader("d3_PIO")))
                    sd3_Parameters.Add("d3_ZO", FormatCommaToPoint(sReader("d3_ZO")))
                    sd3_Parameters.Add("d3_OsiguranjeOdNezaposlenosti", FormatCommaToPoint(sReader("d3_OsiguranjeOdNezaposlenosti")))
                    sd3_Parameters.Add("d3_DodatniDoprinosiZO", FormatCommaToPoint(sReader("d3_DodatniDoprinosiZO")))
                    sd3_Parameters.Add("d3_Prihod", FormatCommaToPoint(sReader("d3_Prihod")))
                    sd3_Parameters.Add("d3_Doprinosi", FormatCommaToPoint(sReader("d3_Doprinosi")))
                    sd3_Parameters.Add("d3_LicniOdbici", FormatCommaToPoint(sReader("d3_LicniOdbici")))
                    sd3_Parameters.Add("d3_Porez", FormatCommaToPoint(sReader("d3_Porez")))
                Catch ex As Exception
                    Dim el As New ErrorsAndEvents.ErrorLogger
                    el.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
                End Try

            End While
            sReader.Close()

        End Using


        sdDokument_Parameters.Add("NazivDokumenta", "Prijava_od_strane_poreznog_obveznika")

        CreateStructure_MIP1023()
        CreateMIP1023_PodaciOPoslodavcu(sd4_Parameters)
        CreateMIP1023_Dio1(sd1_Parameters)
        CreateMIP1023_Dio3(sd3_Parameters)
        CreateMIP1023_Dio4IzjavaPoslodavca(sd4_Parameters)
        CreateMIP1023_Dokument(sdDokument_Parameters)


        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            mycmd = New MySqlCommand
            mycmd.Connection = myconnection


            Dim strSQL As String = <![CDATA[ 
CALL rpt_prz_mip1023_mjes('dp',@pGodina,@pMjesec)
]]>.Value

            mycmd.CommandText = strSQL


            mycmd.Parameters.AddWithValue("@pGodina", sGodina)
            mycmd.Parameters.AddWithValue("@pMjesec", sMjesec)
            mycmd.Prepare()

            sReader = mycmd.ExecuteReader()
            While sReader.Read
                Try
                    sdPodaciOPrihodima_Parameters.Clear()
                    sdPodaciOPrihodima_Parameters.Add("VrstaIsplate", sReader("VrstaIsplate"))
                    sdPodaciOPrihodima_Parameters.Add("Jmb", sReader("Jmb"))
                    sdPodaciOPrihodima_Parameters.Add("ImePrezime", sReader("ImePrezime"))
                    sdPodaciOPrihodima_Parameters.Add("DatumIsplate", sReader("DatumIsplate"))
                    sdPodaciOPrihodima_Parameters.Add("RadniSati", FormatCommaToPoint(sReader("RadniSati")))
                    sdPodaciOPrihodima_Parameters.Add("RadniSatiBolovanje", FormatCommaToPoint(sReader("RadniSatiBolovanje")))
                    sdPodaciOPrihodima_Parameters.Add("BrutoPlaca", FormatCommaToPoint(sReader("BrutoPlaca")))
                    sdPodaciOPrihodima_Parameters.Add("KoristiIDrugiOporeziviPrihodi", FormatCommaToPoint(sReader("KoristiIDrugiOporeziviPrihodi")))
                    sdPodaciOPrihodima_Parameters.Add("UkupanPrihod", FormatCommaToPoint(sReader("UkupanPrihod")))
                    sdPodaciOPrihodima_Parameters.Add("IznosPIO", FormatCommaToPoint(sReader("IznosPIO")))
                    sdPodaciOPrihodima_Parameters.Add("IznosZO", FormatCommaToPoint(sReader("IznosZO")))
                    sdPodaciOPrihodima_Parameters.Add("IznosNezaposlenost", FormatCommaToPoint(sReader("IznosNezaposlenost")))
                    sdPodaciOPrihodima_Parameters.Add("Doprinosi", FormatCommaToPoint(sReader("Doprinosi")))
                    sdPodaciOPrihodima_Parameters.Add("PrihodUmanjenZaDoprinose", FormatCommaToPoint(sReader("PrihodUmanjenZaDoprinose")))
                    sdPodaciOPrihodima_Parameters.Add("FaktorLicnogOdbitka", FormatCommaToPoint(sReader("FaktorLicnogOdbitka")))
                    sdPodaciOPrihodima_Parameters.Add("IznosLicnogOdbitka", FormatCommaToPoint(sReader("IznosLicnogOdbitka")))
                    sdPodaciOPrihodima_Parameters.Add("OsnovicaPoreza", FormatCommaToPoint(sReader("OsnovicaPoreza")))
                    sdPodaciOPrihodima_Parameters.Add("IznosPoreza", FormatCommaToPoint(sReader("IznosPoreza")))
                    sdPodaciOPrihodima_Parameters.Add("RadniSatiUT", FormatCommaToPoint(sReader("RadniSatiUT")))
                    sdPodaciOPrihodima_Parameters.Add("StepenUvecanja", FormatDoubleToInteger(sReader("StepenUvecanja")))
                    sdPodaciOPrihodima_Parameters.Add("SifraRadnogMjestaUT", FormatCommaToPoint(sReader("SifraRadnogMjestaUT")))
                    sdPodaciOPrihodima_Parameters.Add("DoprinosiPIOMIOzaUT", FormatCommaToPoint(sReader("DoprinosiPIOMIOzaUT")))
                    sdPodaciOPrihodima_Parameters.Add("OpcinaPrebivalista", sReader("OpcinaPrebivalista"))
                    sdPodaciOPrihodima_Parameters.Add("BeneficiraniStaz", ConvertStringToLower(sReader("BeneficiraniStaz")))
                    CreateMIP1023_PodaciOPrihodima(sdPodaciOPrihodima_Parameters)
                Catch ex As Exception
                    Dim el As New ErrorsAndEvents.ErrorLogger
                    el.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
                End Try


            End While
            sReader.Close()

        End Using

        Dim sMjeGodObracuna As String = CType(sd1_Parameters("d1_PeriodOd"), String)
        sMjeGodObracuna = "_" + sMjeGodObracuna.Substring(5, 2) + sMjeGodObracuna.Substring(0, 4)
        SaveXmlFile(cFileLocation + "\" + sd4_Parameters("d4_JibJmbPoslodavca") + sMjeGodObracuna + ".xml")

    End Sub

    Public Function FormatCommaToPoint(ByRef sString As String) As String
        Dim rString As String = ""

        Try
            rString = sString.Replace(",", ".")
        Catch ex As Exception
            Dim el As New ErrorsAndEvents.ErrorLogger
            el.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
        Return rString
    End Function

    Public Function ConvertStringToLower(ByRef sString As String) As String
        Dim rString As String = ""

        Try
            rString = sString.ToLower
            Return rString
        Catch ex As Exception
            Dim el As New ErrorsAndEvents.ErrorLogger
            el.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try

        Return rString
    End Function

    Public Function FormatDoubleToInteger(ByRef sString As String) As String
        Dim rString As String = ""
        Dim iString As Integer = 0
        Dim dString As Double = 0
        Try
            ' rString = sString.Replace(",", ".")
            iString = Convert.ToDouble(sString)


            rString = iString.ToString
        Catch ex As Exception
            Dim el As New ErrorsAndEvents.ErrorLogger
            el.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
        Return rString
    End Function
End Class
