Imports System.Xml
Imports System.Reflection
Imports System
Imports MySql.Data
Imports MySql.Data.MySqlClient
Imports Ionic.Zip
Imports MySql.Data.Types

Public Class ClsXmlDocumentGIP

    Private cXmldoc As XmlDocument
    Private cXmlRoot As XmlNode
    Private cXmlTemp As XmlNode
    Private cFileLocation As String
    Private cGodina As Integer

    Dim PodaciOPoslodavcu_Parameters As New Hashtable




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
    Public Property Godina() As Integer
        Get
            Return cGodina
        End Get
        Set(ByVal value As Integer)
            cGodina = value
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
            Return Nothing
        End Try
    End Function

    Public Sub SaveXmlFile(ByVal sXmlFileName As String)
        cXmldoc.Save(sXmlFileName)
    End Sub

    Public Sub CreateStructure_GIP1022()
        cXmlRoot = cXmldoc.CreateElement("PaketniUvozObrazaca")
        cXmldoc.AppendChild(cXmlRoot)

        ' Create an attribute and add it to the root element
        cXmldoc.DocumentElement.SetAttribute("xmlns", "urn:PaketniUvozObrazaca_V1_0.xsd")




        CreateElement(cXmlRoot, "PodaciOPoslodavcu")






    End Sub

    Public Sub CreateStructure_Obrazac1022()
        cXmlTemp = cXmldoc.CreateElement("Obrazac1022")
        'CreateElement(cXmlRoot, "Obrazac1022")

        'CreateElement(FindNodeByName("Obrazac1022"), "Dio1PodaciOPoslodavcuIPoreznomObvezniku")
        'CreateElement(FindNodeByName("Obrazac1022"), "Dio2PodaciOPrihodimaDoprinosimaIPorezu")
        'CreateElement(FindNodeByName("Obrazac1022"), "Dio3IzjavaPoslodavcaIsplatioca")
        'CreateElement(FindNodeByName("Obrazac1022"), "Dokument")

        CreateElement(cXmlTemp, "Dio1PodaciOPoslodavcuIPoreznomObvezniku")
        CreateElement(cXmlTemp, "Dio2PodaciOPrihodimaDoprinosimaIPorezu")
        CreateElement(cXmlTemp, "Dio3IzjavaPoslodavcaIsplatioca")
        CreateElement(cXmlTemp, "Dokument")



    End Sub

    Public Overloads Sub CreateGIP1022_PodaciOPoslodavcu(ByRef sParameters As Hashtable)
        CreateElement(FindNodeByName("PodaciOPoslodavcu"), "JIBPoslodavca", sParameters("JibJmbPoslodavca"))
        CreateElement(FindNodeByName("PodaciOPoslodavcu"), "NazivPoslodavca", sParameters("NazivPoslodavca"))
        CreateElement(FindNodeByName("PodaciOPoslodavcu"), "BrojZahtjeva", sParameters("BrojZahtjeva"))
        CreateElement(FindNodeByName("PodaciOPoslodavcu"), "DatumPodnosenja", sParameters("DatumPodnosenja"))
    End Sub

    Public Overloads Sub CreateGIP1022_Dio1PodaciOPoslodavcuIPoreznomObvezniku(ByRef sParameters As Hashtable, ByVal EmployeeID As Integer)
        Dim sReader As MySql.Data.MySqlClient.MySqlDataReader

        Dim mycmd As MySqlCommand

        Dim nodeDio2 As XmlNode = FindNodeByName("PaketniUvozObrazaca")

        Dim nodeObrazac1022 As XmlNode = CreateElementAndReturnChild(nodeDio2, "Obrazac1022")
        Dim nodeDio1PodaciOPoslodavcuIPoreznomObvezniku As XmlNode = CreateElementAndReturnChild(nodeObrazac1022, "Dio1PodaciOPoslodavcuIPoreznomObvezniku")


        CreateElement(nodeDio1PodaciOPoslodavcuIPoreznomObvezniku, "JIBJMBPoslodavca", sParameters("JibJmbPoslodavca"))
        CreateElement(nodeDio1PodaciOPoslodavcuIPoreznomObvezniku, "Naziv", sParameters("Naziv"))
        CreateElement(nodeDio1PodaciOPoslodavcuIPoreznomObvezniku, "AdresaSjedista", sParameters("AdresaSjedista"))
        CreateElement(nodeDio1PodaciOPoslodavcuIPoreznomObvezniku, "JMBZaposlenika", sParameters("JmbZaposlenika"))
        CreateElement(nodeDio1PodaciOPoslodavcuIPoreznomObvezniku, "ImeIPrezime", sParameters("ImeIPrezime"))
        CreateElement(nodeDio1PodaciOPoslodavcuIPoreznomObvezniku, "AdresaPrebivalista", sParameters("AdresaPrebivalista"))
        CreateElement(nodeDio1PodaciOPoslodavcuIPoreznomObvezniku, "PoreznaGodina", sParameters("PoreznaGodina"))
        CreateElement(nodeDio1PodaciOPoslodavcuIPoreznomObvezniku, "PeriodOd", sParameters("PeriodOd"))
        CreateElement(nodeDio1PodaciOPoslodavcuIPoreznomObvezniku, "PeriodDo", sParameters("PeriodDo"))



        Dim nodeDio2PodaciOPrihodimaDoprinosuIPorezu As XmlNode = CreateElementAndReturnChild(nodeObrazac1022, "Dio2PodaciOPrihodimaDoprinosimaIPorezu")



        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            mycmd = New MySqlCommand
            mycmd.Connection = myconnection


            Dim strSQL As String = <![CDATA[ 
CALL rpt_prz_obracunskilist_xml(@EmployeeID, '-1', @pGodina );
]]>.Value

            mycmd.CommandText = strSQL


            mycmd.Parameters.AddWithValue("@EmployeeID", EmployeeID)
            mycmd.Parameters.AddWithValue("@pGodina", Godina)
            mycmd.Prepare()

            sReader = mycmd.ExecuteReader()
            While sReader.Read
                Try
                    Dim nodePodaciOPrihodimaDoprinosuIPorezu As XmlNode = CreateElementAndReturnChild(nodeDio2PodaciOPrihodimaDoprinosuIPorezu, "PodaciOPrihodimaDoprinosimaIPorezu")
                    CreateElement(nodePodaciOPrihodimaDoprinosuIPorezu, "Mjesec", sReader("porezPeriod"))
                    Dim tmpDate As Date = ConvertMySQLDateToDate(sReader("start_period"))
                    CreateElement(nodePodaciOPrihodimaDoprinosuIPorezu, "IsplataZaMjesecIGodinu", tmpDate.Month.ToString & "/" & tmpDate.Year.ToString)
                    CreateElement(nodePodaciOPrihodimaDoprinosuIPorezu, "VrstaIsplate", "1")
                    CreateElement(nodePodaciOPrihodimaDoprinosuIPorezu, "IznosPrihodaUNovcu", FormatCommaToPoint(sReader("prihodiKM_4")))
                    CreateElement(nodePodaciOPrihodimaDoprinosuIPorezu, "IznosPrihodaUStvarimaUslugama", FormatCommaToPoint(sReader("prihodiStvari_5")))

                    CreateElement(nodePodaciOPrihodimaDoprinosuIPorezu, "BrutoPlaca", FormatCommaToPoint(sReader("brutoPlata_6")))
                    CreateElement(nodePodaciOPrihodimaDoprinosuIPorezu, "IznosZaPenzijskoInvalidskoOsiguranje", FormatCommaToPoint(sReader("iznosPIO_8")))
                    CreateElement(nodePodaciOPrihodimaDoprinosuIPorezu, "IznosZaZdravstvenoOsiguranje", FormatCommaToPoint(sReader("iznosZDR_9")))
                    CreateElement(nodePodaciOPrihodimaDoprinosuIPorezu, "IznosZaOsiguranjeOdNezaposlenosti", FormatCommaToPoint(sReader("iznosZPS_10")))
                    CreateElement(nodePodaciOPrihodimaDoprinosuIPorezu, "UkupniDoprinosi", FormatCommaToPoint(sReader("ukupnoDoprinosi_11")))
                    CreateElement(nodePodaciOPrihodimaDoprinosuIPorezu, "PlacaBezDoprinosa", FormatCommaToPoint(sReader("netoPlata_12")))
                    CreateElement(nodePodaciOPrihodimaDoprinosuIPorezu, "FaktorLicnihOdbitakaPremaPoreznojKartici", FormatCommaToPoint(sReader("faktorOdbitka_13")))
                    CreateElement(nodePodaciOPrihodimaDoprinosuIPorezu, "IznosLicnogOdbitka", FormatCommaToPoint(sReader("iznosOdbitka_14")))
                    CreateElement(nodePodaciOPrihodimaDoprinosuIPorezu, "OsnovicaPoreza", FormatCommaToPoint(sReader("osnovicaPorez_15")))
                    CreateElement(nodePodaciOPrihodimaDoprinosuIPorezu, "IznosUplacenogPoreza", FormatCommaToPoint(sReader("iznosPorez_16")))
                    CreateElement(nodePodaciOPrihodimaDoprinosuIPorezu, "NetoPlaca", FormatCommaToPoint(sReader("iznosPlate_17")))
                    CreateElement(nodePodaciOPrihodimaDoprinosuIPorezu, "DatumUplate", sReader("datIsplate_2s"))
                Catch ex As Exception
                    Dim el As New ErrorsAndEvents.ErrorLogger
                    el.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
                End Try
                sReader.Close()

            End While

        End Using



        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            mycmd = New MySqlCommand
            mycmd.Connection = myconnection


            Dim strSQL As String = <![CDATA[ 
CALL rpt_prz_obracunskilist_xml('SUM', @EmployeeID, @pGodina );
]]>.Value

            mycmd.CommandText = strSQL


            mycmd.Parameters.AddWithValue("@EmployeeID", EmployeeID)
            mycmd.Parameters.AddWithValue("@pGodina", Godina)
            mycmd.Prepare()

            sReader = mycmd.ExecuteReader()
            While sReader.Read
                Try
                    Dim nodeUkupno As XmlNode = CreateElementAndReturnChild(nodeDio2PodaciOPrihodimaDoprinosuIPorezu, "Ukupno")
                    CreateElement(nodeUkupno, "IznosPrihodaUNovcu", FormatCommaToPoint(sReader("prihodiKM_4")))
                    CreateElement(nodeUkupno, "IznosPrihodaUStvarimaUslugama", FormatCommaToPoint(sReader("prihodiStvari_5")))

                    CreateElement(nodeUkupno, "BrutoPlaca", FormatCommaToPoint(sReader("brutoPlata_6")))
                    CreateElement(nodeUkupno, "IznosZaPenzijskoInvalidskoOsiguranje", FormatCommaToPoint(sReader("iznosPIO_8")))
                    CreateElement(nodeUkupno, "IznosZaZdravstvenoOsiguranje", FormatCommaToPoint(sReader("iznosZDR_9")))
                    CreateElement(nodeUkupno, "IznosZaOsiguranjeOdNezaposlenosti", FormatCommaToPoint(sReader("iznosZPS_10")))
                    CreateElement(nodeUkupno, "UkupniDoprinosi", FormatCommaToPoint(sReader("ukupnoDoprinosi_11")))
                    CreateElement(nodeUkupno, "PlacaBezDoprinosa", FormatCommaToPoint(sReader("netoPlata_12")))

                    CreateElement(nodeUkupno, "IznosLicnogOdbitka", FormatCommaToPoint(sReader("iznosOdbitka_14")))
                    CreateElement(nodeUkupno, "OsnovicaPoreza", FormatCommaToPoint(sReader("osnovicaPorez_15")))
                    CreateElement(nodeUkupno, "IznosUplacenogPoreza", FormatCommaToPoint(sReader("iznosPorez_16")))
                    CreateElement(nodeUkupno, "NetoPlaca", FormatCommaToPoint(sReader("iznosPlate_17")))
                Catch ex As Exception
                    Dim el As New ErrorsAndEvents.ErrorLogger
                    el.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
                End Try
                sReader.Close()

            End While

        End Using

        Dim nodeDio3IzjavaPoslodavcaIsplatioca As XmlNode = CreateElementAndReturnChild(nodeObrazac1022, "Dio3IzjavaPoslodavcaIsplatioca")
        CreateElement(nodeDio3IzjavaPoslodavcaIsplatioca, "JIBJMBPoslodavca", sParameters("JibJmbPoslodavca"))
        CreateElement(nodeDio3IzjavaPoslodavcaIsplatioca, "DatumUnosa", Now.ToString("yyyy-MM-dd"))
        CreateElement(nodeDio3IzjavaPoslodavcaIsplatioca, "NazivPoslodavca", sParameters("NazivPoslodavca"))


        Dim nodeDokument As XmlNode = CreateElementAndReturnChild(nodeObrazac1022, "Dokument")
        ' Bug-Evernote link: https://www.evernote.com/shard/s426/nl/71155386/22342a77-4c2c-4bb8-a229-9a3816238caf
        'CreateElement(nodeDokument, "BrojDokumenta", "")
        'CreateElement(nodeDokument, "Sluzbenik", "")
        CreateElement(nodeDokument, "Operacija", "Novi")
        'CreateElement(nodeDokument, "DatumPrijema", "")

    End Sub


    Public Overloads Sub CreateGIP1022_Dio3IzjavaPoslodavcaIsplatioca(ByRef sParameters As Hashtable)

        CreateElement(FindNodeByName("Dio3IzjavaPoslodavcaIsplatioca"), "JibJmbPoslodavca", sParameters("PorBroj"))
        CreateElement(FindNodeByName("Dio3IzjavaPoslodavcaIsplatioca"), "DatumUnosa", "")
        CreateElement(FindNodeByName("Dio3IzjavaPoslodavcaIsplatioca"), "NazivPoslodavca", sParameters("Naziv"))
    End Sub

    Public Overloads Sub CreateGIP1022_Dokument(ByRef sParameters As Hashtable)

        CreateElement(FindNodeByName("Dokument"), "Operacija", sParameters("NazivDokumenta"))

    End Sub

    Public Overloads Sub CreateGIP1022_PodaciOPrihodima(ByRef sParameters As Hashtable)

        Dim nodeDio2 As XmlNode = FindNodeByName("Dio2PodaciOPrihodimaDoprinosimaIPorezu")

        Dim nodePodaciOPrihodima As XmlNode = CreateElementAndReturnChild(nodeDio2, "PodaciOPrihodimaDoprinosimaIPorezu")

        CreateElement(nodePodaciOPrihodima, "Mjesec", sParameters("Mjesec"))
        CreateElement(nodePodaciOPrihodima, "IsplataZaMjesecIGodinu", sParameters("IsplataZaMjesecIGodinu"))
        CreateElement(nodePodaciOPrihodima, "VrstaIsplate", sParameters("VrstaIsplate"))
        CreateElement(nodePodaciOPrihodima, "IznosPrihodaUNovcu", sParameters("IznosPrihodaUNovcu"))
        CreateElement(nodePodaciOPrihodima, "IznosPrihodaUStvarimaUslugama", sParameters("IznosPrihodaUStvarimaUslugama"))
        CreateElement(nodePodaciOPrihodima, "BrutoPlaca", sParameters("BrutoPlaca"))
        CreateElement(nodePodaciOPrihodima, "IznosZaPenzijskoInvalidskoOsiguranje", sParameters("IznosZaPenzijskoInvalidskoOsiguranje"))
        CreateElement(nodePodaciOPrihodima, "IznosZaZdravstvenoOsiguranje", sParameters("IznosZaZdravstvenoOsiguranje"))
        CreateElement(nodePodaciOPrihodima, "IznosZaOsiguranjeOdNezaposlenosti", sParameters("IznosZaOsiguranjeOdNezaposlenosti"))
        CreateElement(nodePodaciOPrihodima, "UkupniDoprinosi", sParameters("UkupniDoprinosi"))
        CreateElement(nodePodaciOPrihodima, "PlacaBezDoprinosa", sParameters("PlacaBezDoprinosa"))
        CreateElement(nodePodaciOPrihodima, "FaktorLicnihOdbitakaPremaPoreznojKartici", sParameters("FaktorLicnihOdbitakaPremaPoreznojKartici"))
        CreateElement(nodePodaciOPrihodima, "IznosLicnogOdbitka", sParameters("IznosLicnogOdbitka"))
        CreateElement(nodePodaciOPrihodima, "OsnovicaPoreza", sParameters("OsnovicaPoreza"))
        CreateElement(nodePodaciOPrihodima, "IznosUplacenogPoreza", sParameters("IznosUplacenogPoreza"))
        CreateElement(nodePodaciOPrihodima, "NetoPlaca", sParameters("NetoPlaca"))
        CreateElement(nodePodaciOPrihodima, "DatumUplate", sParameters("DatumUplate"))

    End Sub

    Public Overloads Sub CreateGIP1022_Ukupno(ByRef sParameters As Hashtable)

        Dim nodeDio2 As XmlNode = FindNodeByName("Dio2PodaciOPrihodimaDoprinosimaIPorezu")

        Dim nodePodaciOPrihodima As XmlNode = CreateElementAndReturnChild(nodeDio2, "Ukupno")

        CreateElement(nodePodaciOPrihodima, "IznosPrihodaUNovcu", sParameters("IznosPrihodaUNovcu"))
        CreateElement(nodePodaciOPrihodima, "IznosPrihodaUStvarimaUslugama", sParameters("IznosPrihodaUStvarimaUslugama"))
        CreateElement(nodePodaciOPrihodima, "BrutoPlaca", sParameters("BrutoPlaca"))
        CreateElement(nodePodaciOPrihodima, "IznosZaPenzijskoInvalidskoOsiguranje", sParameters("IznosZaPenzijskoInvalidskoOsiguranje"))
        CreateElement(nodePodaciOPrihodima, "IznosZaZdravstvenoOsiguranje", sParameters("IznosZaZdravstvenoOsiguranje"))
        CreateElement(nodePodaciOPrihodima, "IznosZaOsiguranjeOdNezaposlenosti", sParameters("IznosZaOsiguranjeOdNezaposlenosti"))
        CreateElement(nodePodaciOPrihodima, "UkupniDoprinosi", sParameters("UkupniDoprinosi"))
        CreateElement(nodePodaciOPrihodima, "PlacaBezDoprinosa", sParameters("PlacaBezDoprinosa"))
        CreateElement(nodePodaciOPrihodima, "FaktorLicnihOdbitakaPremaPoreznojKartici", sParameters("FaktorLicnihOdbitakaPremaPoreznojKartici"))
        CreateElement(nodePodaciOPrihodima, "IznosLicnogOdbitka", sParameters("IznosLicnogOdbitka"))
        CreateElement(nodePodaciOPrihodima, "OsnovicaPoreza", sParameters("OsnovicaPoreza"))
        CreateElement(nodePodaciOPrihodima, "IznosUplacenogPoreza", sParameters("IznosUplacenogPoreza"))
        CreateElement(nodePodaciOPrihodima, "NetoPlaca", sParameters("NetoPlaca"))

    End Sub


    Public Sub FillGIP1022()
        Dim sReader As MySql.Data.MySqlClient.MySqlDataReader

        Dim mycmd As MySqlCommand

        Dim Dio1PodaciOPoslodavcuIPoreznomObvezniku_Parameters As New Hashtable
        'Dim PodaciOPoslodavcu_Parameters As New Hashtable
        Dim Dio3IzjavaPoslodavcaIsplatioca_Parameters As New Hashtable
        Dim sdDokument_Parameters As New Hashtable
        Dim sdPodaciOPrihodima_Parameters As New Hashtable

        CreateStructure_GIP1022()


        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            mycmd = New MySqlCommand
            mycmd.Connection = myconnection


            Dim strSQL As String = <![CDATA[ 
CALL rpt_prz_obracunskilist_xml('d1','-1', @pGodina );
]]>.Value

            mycmd.CommandText = strSQL


            mycmd.Parameters.AddWithValue("@pGodina", Godina)
            mycmd.Prepare()

            sReader = mycmd.ExecuteReader()
            While sReader.Read
                Try
                    PodaciOPoslodavcu_Parameters.Add("JibJmbPoslodavca", sReader("PorBroj"))
                    PodaciOPoslodavcu_Parameters.Add("NazivPoslodavca", sReader("Naziv"))
                    PodaciOPoslodavcu_Parameters.Add("BrojZahtjeva", sReader("BrojZahtjeva"))
                    PodaciOPoslodavcu_Parameters.Add("DatumPodnosenja", sReader("DatumPodnosenja"))
                Catch ex As Exception
                    Dim el As New ErrorsAndEvents.ErrorLogger
                    el.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
                End Try
            End While
            sReader.Close()

        End Using


        CreateGIP1022_PodaciOPoslodavcu(PodaciOPoslodavcu_Parameters)



        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            mycmd = New MySqlCommand
            mycmd.Connection = myconnection


            Dim strSQL As String = <![CDATA[ 
CALL rpt_prz_obracunskilist_xml('d2','-1', @pGodina );
]]>.Value

            mycmd.CommandText = strSQL


            mycmd.Parameters.AddWithValue("@pGodina", Godina)
            mycmd.Prepare()

            sReader = mycmd.ExecuteReader()
            While sReader.Read
                Try
                    Dio1PodaciOPoslodavcuIPoreznomObvezniku_Parameters.Add("JibJmbPoslodavca", sReader("PorBroj"))
                    Dio1PodaciOPoslodavcuIPoreznomObvezniku_Parameters.Add("Naziv", sReader("Naziv"))
                    Dio1PodaciOPoslodavcuIPoreznomObvezniku_Parameters.Add("AdresaSjedista", sReader("AdresaOrg"))
                    Dio1PodaciOPoslodavcuIPoreznomObvezniku_Parameters.Add("JmbZaposlenika", sReader("JMB"))
                    Dio1PodaciOPoslodavcuIPoreznomObvezniku_Parameters.Add("ImeIPrezime", sReader("ImeIPrezime"))
                    Dio1PodaciOPoslodavcuIPoreznomObvezniku_Parameters.Add("AdresaPrebivalista", sReader("AdresaPrebivalista"))
                    Dio1PodaciOPoslodavcuIPoreznomObvezniku_Parameters.Add("PoreznaGodina", sReader("PoreznaGodina"))
                    Dio1PodaciOPoslodavcuIPoreznomObvezniku_Parameters.Add("PeriodOd", sReader("PeriodOd"))
                    Dio1PodaciOPoslodavcuIPoreznomObvezniku_Parameters.Add("PeriodDo", sReader("PeriodDo"))
                    Dim EmployeeID As Integer = sReader("EmployeeID")
                    CreateGIP1022_Dio1PodaciOPoslodavcuIPoreznomObvezniku(Dio1PodaciOPoslodavcuIPoreznomObvezniku_Parameters, EmployeeID)
                    Dio1PodaciOPoslodavcuIPoreznomObvezniku_Parameters.Clear()
                Catch ex As Exception
                    Dim el As New ErrorsAndEvents.ErrorLogger
                    el.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
                End Try
                sReader.Close()

            End While

        End Using



        sdDokument_Parameters.Add("NazivDokumenta", "Prijava_od_strane_poreznog_obveznika")



        'CreateGIP1022_Dio1PodaciOPoslodavcuIPoreznomObvezniku(Dio1PodaciOPoslodavcuIPoreznomObvezniku_Parameters)
        'CreateGIP1022_Dio3IzjavaPoslodavcaIsplatioca(Dio3IzjavaPoslodavcaIsplatioca_Parameters)
        'CreateGIP1022_Dio3IzjavaPoslodavcaIsplatioca(PodaciOPoslodavcu_Parameters)
        'CreateGIP1022_Dokument(sdDokument_Parameters)


        Dim sMjeGodObracuna As String = CType(Dio1PodaciOPoslodavcuIPoreznomObvezniku_Parameters("PeriodOd"), String)
        'sMjeGodObracuna = "_" + sMjeGodObracuna.Substring(0, 4)
        SaveXmlFile(cFileLocation + "\" + PodaciOPoslodavcu_Parameters("JibJmbPoslodavca") + ".xml")
        ZiplFile(cFileLocation + "\" + PodaciOPoslodavcu_Parameters("JibJmbPoslodavca") + ".xml",
                        cFileLocation + "\" + PodaciOPoslodavcu_Parameters("JibJmbPoslodavca") + "_1022" + ".zip")

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

    Public Function ConvertMySQLDateToDate(ByRef sDate As MySqlDateTime) As Date
        Dim rDate As Date

        Try
            rDate = CType(sDate, Date)
            Return rDate
        Catch ex As Exception
            Dim el As New ErrorsAndEvents.ErrorLogger
            el.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
            'MsgBox("Error logged.")
        End Try
        rDate = Date.Today
        Return rDate
    End Function

    Public Sub ZiplFile(ByVal FileToZip As String, ByVal FileZipName As String)
        Try
            Using zip1 As ZipFile = New ZipFile
                ' Upiši fajl bez arhivskog foldera
                zip1.AddFile(FileToZip, "")
                zip1.Save(FileZipName)
            End Using
        Catch ex As Exception
            Dim el As New ErrorsAndEvents.ErrorLogger
            el.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try

    End Sub

End Class
