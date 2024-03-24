Imports Syncfusion.DocIO.DLS
Imports Syncfusion.DocIO
Imports Syncfusion.Windows.Forms
Imports MySql.Data.MySqlClient
Imports MySql.Data
Imports System.Data
Imports System.Web.Configuration
Imports System.Drawing

Public Class ClsSumaEvidencije

    Public cDataAdapter As MySqlDataAdapter
    Public cDataSet As New DataSet
    Private cDbTableName As String

    Public cDataSetLock As New DataSet
    Private cDbTableNameLock As String

    Private cOrgSfr As String = ""

    Private cSumaSatiNula As Boolean = True
    Private cColsRW As New Dictionary(Of Integer, String)

    Private sfrPlacLista As New Dictionary(Of String, ClsSfrPlacItem)

    Public cSfrPlacLock As String = ""

    Private cAutoload As String = "obracun.autoload"
    Private cSfrPlacLockException As String = ""

    ' Preklapanje GO sa BOL, POD, NPL, PLC
    '
    Public cprklpGoLst As New List(Of Integer)

#Region "Property"



    Public ReadOnly Property DataSet() As DataSet
        Get
            Return cDataSet
        End Get
    End Property


    Public ReadOnly Property TableColsRW() As Dictionary(Of Integer, String)
        Get
            Return cColsRW
        End Get
    End Property

    Public Property TableName() As String
        Get
            Return cDbTableName
        End Get
        Set(ByVal value As String)
            cDbTableName = "tmp_evd_" + value
            cOrgSfr = value
        End Set
    End Property

    Public Property TableNameLock() As String
        Get
            Return cDbTableNameLock
        End Get
        Set(ByVal value As String)
            cDbTableName = "tmp_evd_" + value + "_lock"
            cOrgSfr = value
        End Set
    End Property

    Public ReadOnly Property SumaSatiNula() As Boolean
        Get
            Return cSumaSatiNula
        End Get
    End Property

    Public ReadOnly Property SifranikPlacanja() As Dictionary(Of String, ClsSfrPlacItem)
        Get
            Return sfrPlacLista
        End Get
    End Property

    Public Property Autoload As String
        Get
            Return cAutoload
        End Get
        Set(ByVal value As String)
            cAutoload = value
        End Set
    End Property

    Public Property LockException As String
        Get
            Return cSfrPlacLockException
        End Get
        Set(ByVal value As String)
            cSfrPlacLockException = value
        End Set
    End Property
#End Region

    ''' <summary>
    ''' Load iz tmp_evd_xxx_yy tabele
    ''' </summary>
    ''' <param name="pMM"></param>
    ''' <param name="pGGGG"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function LoadDbEvid(ByVal pMM As Integer, ByVal pGGGG As Integer) As DataSet
        Dim sqlText As String
        Dim dbResultInt As Integer

        Try
            sqlText = <sql text="CALL `EVD_create_tmp_evd_3`('','?DbTableName','?MM','?GGGG');"/>.Attribute("text").Value
            sqlText = sqlText.Replace("?DbTableName", cOrgSfr)
            sqlText = sqlText.Replace("?MM", pMM)
            sqlText = sqlText.Replace("?GGGG", pGGGG)

            Using myconnection As New MySqlConnection(ConnectionString)
                myconnection.Open()

                Dim mycmd As New MySqlCommand
                mycmd.Connection = myconnection

                'strSQL = strSQL.Replace("%__Days__%", pDays)
                mycmd.CommandText = sqlText


                dbResultInt = mycmd.ExecuteNonQuery()

                sqlText = <sql text="CALL `EVD_update_tmp_evd`('','?DbTableName','?MM','?GGGG');"/>.Attribute("text").Value
                sqlText = sqlText.Replace("?DbTableName", cOrgSfr)
                sqlText = sqlText.Replace("?MM", pMM)
                sqlText = sqlText.Replace("?GGGG", pGGGG)
                'strSQL = strSQL.Replace("%__Days__%", pDays)
                mycmd.CommandText = sqlText

                mycmd.ExecuteNonQuery()



                sqlText = <sql text="SELECT  * FROM ?DbTableName ORDER BY Uposlenik;"/>.Attribute("text").Value
                sqlText = sqlText.Replace("?DbTableName", cDbTableName)

                cDataSet.Tables.Clear()


                cDataAdapter = New MySqlDataAdapter(sqlText, myconnection)
                Dim cb As MySqlCommandBuilder = New MySqlCommandBuilder(cDataAdapter)
                    'cDataAdapter.UpdateCommand = cb.GetUpdateCommand
                    'cDataAdapter.TableMappings.Add(cDbTableName, cDbTableName)
                    cDbTableNameLock = cDbTableName + "_lock"
                    cDataSet.Tables.Add(cDbTableName)

                    cDataAdapter.Fill(cDataSet.Tables(cDbTableName))

            End Using
        Catch ex As Exception
            Dim el As New ErrorsAndEvents.ErrorLogger
            el.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try


        CheckSumaSati(pMM, pGGGG)

        Return cDataSet

    End Function

    Public Sub LoadDbEvid_Lock(ByVal pMM As Integer, ByVal pGGGG As Integer)
        Dim sqlText As String
        Dim dbResultInt As Integer

        cDbTableNameLock = cDbTableName + "_lock"
        Try
            sqlText = <sql text="CALL `EVD_create_tmp_evd_lock`('','?DbTableName','?MM','?GGGG');"/>.Attribute("text").Value
            sqlText = sqlText.Replace("?DbTableName", cOrgSfr)
            sqlText = sqlText.Replace("?MM", pMM)
            sqlText = sqlText.Replace("?GGGG", pGGGG)

            Using myconnection As New MySqlConnection(ConnectionString)
                myconnection.Open()

                Dim mycmd As New MySqlCommand
                mycmd.Connection = myconnection

                'strSQL = strSQL.Replace("%__Days__%", pDays)
                mycmd.CommandText = sqlText

                mycmd.ExecuteNonQuery()
            End Using
        Catch ex As Exception
            Dim el As New ErrorsAndEvents.ErrorLogger
            el.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try



        sqlText = <sql text="SELECT  * FROM ?DbTableName ORDER BY Uposlenik;"/>.Attribute("text").Value
        sqlText = sqlText.Replace("?DbTableName", cDbTableNameLock)

        'cDataSet.Tables.Remove(cDbTableNameLock)

        Try
            cDataSetLock.Tables.Clear()

            Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString
            Dim _DataAdapter As MySqlDataAdapter = New MySqlDataAdapter(sqlText, New MySqlConnection(ConnectionString))
            _DataAdapter.SelectCommand.CommandText = sqlText
            cDataSetLock.Tables.Add(cDbTableNameLock)

            _DataAdapter.Fill(cDataSetLock.Tables(cDbTableNameLock))

        Catch ex As Exception

        End Try

        'cDataSet.Tables(cDbTableNameLock)


    End Sub

    Public Sub LoadDbEvid_GoPrkl(ByVal pMM As Integer, ByVal pGGGG As Integer)
        Dim sqlText As String

        cDbTableNameLock = cDbTableName + "_lock"
        Try
            sqlText = <sql text="CALL `EVD_create_tmp_evd_goprkl`('','?DbTableName','?MM','?GGGG');"/>.Attribute("text").Value
            sqlText = sqlText.Replace("?DbTableName", cOrgSfr)
            sqlText = sqlText.Replace("?MM", pMM)
            sqlText = sqlText.Replace("?GGGG", pGGGG)


            cprklpGoLst.Clear()

            Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
                myconnection.Open()

                Dim mycmd As New MySql.Data.MySqlClient.MySqlCommand
                Dim dbRead As MySqlClient.MySqlDataReader

                mycmd.Connection = myconnection

                mycmd.CommandText = sqlText


                Try
                    dbRead = mycmd.ExecuteReader
                    Do While dbRead.Read
                        cprklpGoLst.Add((dbRead("employeeID")))
                    Loop
                    dbRead.Close()

                Catch ex As Exception
                    Console.WriteLine(ex.Message)
                End Try

            End Using

        Catch ex As Exception
            Dim el As New ErrorsAndEvents.ErrorLogger
            el.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try




    End Sub

    ''' <summary>
    ''' Provjera da li su odobreni redovi tabele evd_prisustva_suma
    ''' Yes - cSumaSatiNula=False, No - cSumaSatiNula=True
    ''' </summary>
    ''' <param name="pMM"></param>
    ''' <param name="pGGGG"></param>
    ''' <remarks></remarks>
    Public Sub CheckSumaSati(ByVal pMM As Integer, ByVal pGGGG As Integer)
        Dim sqlText As String
        Dim dbResult As Double



        sqlText = <sql text="CALL `EVD_suma_evd_prissuma`('?DbTableName','?MM','?GGGG');"/>.Attribute("text").Value
        sqlText = sqlText.Replace("?DbTableName", cOrgSfr)
        sqlText = sqlText.Replace("?MM", pMM)
        sqlText = sqlText.Replace("?GGGG", pGGGG)


        Try

            Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
                myconnection.Open()

                Dim mycmd As New MySql.Data.MySqlClient.MySqlCommand
                Dim dbRead As MySqlClient.MySqlDataReader

                mycmd.Connection = myconnection

                mycmd.CommandText = sqlText


                Try
                    dbRead = mycmd.ExecuteReader
                    Do While dbRead.Read
                        cprklpGoLst.Add((dbRead.GetDecimal("SumaSati")))
                    Loop
                    dbRead.Close()

                Catch ex As Exception
                    Console.WriteLine(ex.Message)
                End Try

            End Using


        Catch ex As Exception

        End Try

        If dbResult > 0 Then
            cSumaSatiNula = False
        Else
            cSumaSatiNula = True
        End If

    End Sub

    Public Sub Zakljuci(ByVal pMM As Integer, ByVal pGGGG As Integer, ByVal pUserId As Integer)
        Dim sqlText As String
        Dim dbResultInt As Integer



        sqlText = <sql text="CALL `EVD_odobri_evd_prissuma`('','?DbTableName','?MM','?GGGG', '?UserId' );"/>.Attribute("text").Value
        sqlText = sqlText.Replace("?DbTableName", cOrgSfr)
        sqlText = sqlText.Replace("?MM", pMM)
        sqlText = sqlText.Replace("?GGGG", pGGGG)
        sqlText = sqlText.Replace("?UserId", pUserId)

        Try
            Using myconnection As New MySqlConnection(ConnectionString)
                myconnection.Open()

                Dim mycmd As New MySqlCommand
                mycmd.Connection = myconnection

                'strSQL = strSQL.Replace("%__Days__%", pDays)
                mycmd.CommandText = sqlText


                dbResultInt = mycmd.ExecuteNonQuery()
            End Using

        Catch ex As Exception
            Dim el As New ErrorsAndEvents.ErrorLogger
            el.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try

    End Sub

    'Public Sub VratiZakljuceno(ByVal pMM As Integer, ByVal pGGGG As Integer, ByVal pUserId As Integer)
    '    Dim sqlText As String
    '    Dim dbResultInt As Integer



    '    sqlText = <sql text="CALL `EVD_odobri_evd_prissuma`('','?DbTableName','?MM','?GGGG', '?UserId' );"/>.Attribute("text").Value
    '    sqlText = sqlText.Replace("?DbTableName", cOrgSfr)
    '    sqlText = sqlText.Replace("?MM", pMM)
    '    sqlText = sqlText.Replace("?GGGG", pGGGG)
    '    sqlText = sqlText.Replace("?UserId", pUserId)

    '    Try
    '        ClsDatabaseGeneral.ExecuteSQL_Return(sqlText, dbResultInt)

    '    Catch ex As Exception
    '        Dim el As New ErrorsAndEvents.ErrorLogger
    '        el.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
    '    End Try

    'End Sub

    Public Sub Lock(ByVal pMM As Integer, ByVal pGGGG As Integer, ByVal pUserId As Integer)
        Dim sqlText As String
        Dim dbResultInt As Integer



        sqlText = <sql text="CALL `EVD_lock_evd_prissuma`('','?DbTableName','?MM','?GGGG', '?UserId', '?cSfrPlacLock' );"/>.Attribute("text").Value
        sqlText = sqlText.Replace("?DbTableName", cOrgSfr)
        sqlText = sqlText.Replace("?MM", pMM.ToString)
        sqlText = sqlText.Replace("?GGGG", pGGGG.ToString)
        sqlText = sqlText.Replace("?UserId", pUserId.ToString)
        sqlText = sqlText.Replace("?cSfrPlacLock", cSfrPlacLock)

        Try
            Using myconnection As New MySqlConnection(ConnectionString)
                myconnection.Open()

                Dim mycmd As New MySqlCommand
                mycmd.Connection = myconnection

                'strSQL = strSQL.Replace("%__Days__%", pDays)
                mycmd.CommandText = sqlText


                dbResultInt = mycmd.ExecuteNonQuery()
            End Using

        Catch ex As Exception
            Dim el As New ErrorsAndEvents.ErrorLogger
            el.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try

    End Sub

    Public Sub Spasi(ByVal pMM As Integer, ByVal pGGGG As Integer, ByVal pUserId As Integer)
        Dim sqlText As String
        Dim dbResultInt As Integer



        sqlText = <sql text="CALL `EVD_insert_evd_prissuma`('','?DbTableName','?MM','?GGGG', '?UserId' );"/>.Attribute("text").Value
        sqlText = sqlText.Replace("?DbTableName", cOrgSfr)
        sqlText = sqlText.Replace("?MM", pMM)
        sqlText = sqlText.Replace("?GGGG", pGGGG)
        sqlText = sqlText.Replace("?UserId", pUserId)

        Try

            Using myconnection As New MySqlConnection(ConnectionString)
                myconnection.Open()

                Dim mycmd As New MySqlCommand
                mycmd.Connection = myconnection

                'strSQL = strSQL.Replace("%__Days__%", pDays)
                mycmd.CommandText = sqlText

                dbResultInt = mycmd.ExecuteNonQuery()
            End Using

        Catch ex As Exception
            Dim el As New ErrorsAndEvents.ErrorLogger
            el.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try

        sqlText = <sql text="CALL `EVD_update_evd_prissuma_2`('','?DbTableName','?MM','?GGGG', '?UserId' );"/>.Attribute("text").Value
        sqlText = sqlText.Replace("?DbTableName", cOrgSfr)
        sqlText = sqlText.Replace("?MM", pMM)
        sqlText = sqlText.Replace("?GGGG", pGGGG)
        sqlText = sqlText.Replace("?UserId", pUserId)

        Try

            Using myconnection As New MySqlConnection(ConnectionString)
                myconnection.Open()

                Dim mycmd As New MySqlCommand
                mycmd.Connection = myconnection

                'strSQL = strSQL.Replace("%__Days__%", pDays)
                mycmd.CommandText = sqlText


                dbResultInt = mycmd.ExecuteNonQuery()
            End Using

        Catch ex As Exception
            Dim el As New ErrorsAndEvents.ErrorLogger
            el.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try

    End Sub

    Public Sub Proracunaj(ByVal pMM As Integer, ByVal pGGGG As Integer)
        Dim sqlText As String
        Dim dbResultInt As Integer



        sqlText = <sql text="CALL `EVD_calculate_tmp_evd`('?DbTableName','?MM','?GGGG');"/>.Attribute("text").Value
        sqlText = sqlText.Replace("?DbTableName", cOrgSfr)
        sqlText = sqlText.Replace("?MM", pMM)
        sqlText = sqlText.Replace("?GGGG", pGGGG)


        Try

            Using myconnection As New MySqlConnection(ConnectionString)
                myconnection.Open()

                Dim mycmd As New MySqlCommand
                mycmd.Connection = myconnection

                'strSQL = strSQL.Replace("%__Days__%", pDays)
                mycmd.CommandText = sqlText


                dbResultInt = mycmd.ExecuteNonQuery()
            End Using

        Catch ex As Exception
            Dim el As New ErrorsAndEvents.ErrorLogger
            el.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try

    End Sub

    '''' <summary>
    '''' Autoload evidencija
    '''' </summary>
    '''' <param name="pMM"></param>
    '''' <param name="pGGGG"></param>
    'Public Sub Preuzmi(ByVal pMM As Integer, ByVal pGGGG As Integer)
    '    Dim sqlText As String
    '    Dim dbResultInt As Integer



    '    sqlText = <sql text="CALL `EVD_autoload_tmp_evd`('?DbTableName','?MM','?GGGG');"/>.Attribute("text").Value
    '    sqlText = sqlText.Replace("?DbTableName", cOrgSfr)
    '    sqlText = sqlText.Replace("?MM", pMM)
    '    sqlText = sqlText.Replace("?GGGG", pGGGG)


    '    Try
    '        ClsDatabaseGeneral.ExecuteSQL_Return(sqlText, dbResultInt)

    '    Catch ex As Exception
    '        Dim el As New ErrorsAndEvents.ErrorLogger
    '        el.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
    '    End Try

    'End Sub

    Public Sub Preuzmi(ByVal pMM As Integer, ByVal pGGGG As Integer, ByVal pAutoEnable As String)
        Dim sqlText As String
        Dim dbResultInt As Integer



        sqlText = <sql text="CALL `EVD_autoload2_tmp_evd`('?DbTableName','?MM','?GGGG', '?AutoEnable');"/>.Attribute("text").Value
        sqlText = sqlText.Replace("?DbTableName", cOrgSfr)
        sqlText = sqlText.Replace("?MM", pMM)
        sqlText = sqlText.Replace("?GGGG", pGGGG)
        sqlText = sqlText.Replace("?AutoEnable", pAutoEnable)

        Try
            Using myconnection As New MySqlConnection(ConnectionString)
                myconnection.Open()

                Dim mycmd As New MySqlCommand
                mycmd.Connection = myconnection

                'strSQL = strSQL.Replace("%__Days__%", pDays)
                mycmd.CommandText = sqlText

                dbResultInt = mycmd.ExecuteNonQuery()
            End Using

        Catch ex As Exception
            Dim el As New ErrorsAndEvents.ErrorLogger
            el.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try

    End Sub

    Public Sub PreuzmiSate(ByVal pMM As Integer, ByVal pGGGG As Integer, Optional ByVal pAutoEnable As String = "")
        Dim sqlText As String
        Dim dbResultInt As Integer


        sqlText = <sql text="CALL `EVD_autoload_sched_tmp_evd`('?DbTableName','?MM','?GGGG', '?AutoEnable');"/>.Attribute("text").Value
        sqlText = sqlText.Replace("?DbTableName", cOrgSfr)
        sqlText = sqlText.Replace("?MM", pMM)
        sqlText = sqlText.Replace("?GGGG", pGGGG)
        sqlText = sqlText.Replace("?AutoEnable", pAutoEnable)

        Try
            Using myconnection As New MySqlConnection(ConnectionString)
                myconnection.Open()

                Dim mycmd As New MySqlCommand
                mycmd.Connection = myconnection

                'strSQL = strSQL.Replace("%__Days__%", pDays)
                mycmd.CommandText = sqlText


                dbResultInt = mycmd.ExecuteNonQuery()
            End Using

        Catch ex As Exception
            Dim el As New ErrorsAndEvents.ErrorLogger
            el.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try

    End Sub

    ''' <summary>
    ''' Mapira nazive kolona u nove nazive koji odgovaraju nazivu plaćanja
    ''' Dodaje novi red kolona u kojima su Sifre plaćanja
    ''' </summary>
    ''' <param name="pGridDataBoundGrid"></param>
    ''' <remarks></remarks>
    Public Sub LoadSifarnik(ByVal pVrstaRWRole As String)
        Dim sqlText As String
        sqlText = <sql text="SELECT sp.id, sp.`Sifra`, sp.`Naziv`, sp.`Vrsta`  FROM `sfr_placanja` sp;"/>.Attribute("text").Value
        'sqlText = sqlText.Replace("?employeeID", EmployeeId)

        ' Sadrži podatke o kolonama plaćanja iz role usera
        Dim sVrstaRW As New Dictionary(Of String, String)
        'Dim sVrstaRWRole As String = "RAD.RW, ODS.RW, BOL.R, TPL.RW"
        Dim parts As String() = pVrstaRWRole.Split(",")

        For Each el In parts
            Dim parts1 As String() = el.Split(".")
            sVrstaRW.Add(Trim(parts1(0)), Trim(parts1(1)))
        Next

        'sVrstaRW.Add("RAD", "RW")
        'sVrstaRW.Add("ODS", "RW")
        'sVrstaRW.Add("BOL", "R")

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySql.Data.MySqlClient.MySqlCommand
            Dim dbRead As MySqlClient.MySqlDataReader

            mycmd.Connection = myconnection

            mycmd.CommandText = sqlText


            Try
                dbRead = mycmd.ExecuteReader
                Do While dbRead.Read
                    Dim itemSfrPlac As New ClsSfrPlacItem
                    itemSfrPlac.UniqueId = dbRead("id")
                    itemSfrPlac.Sifra = dbRead.GetString("Sifra")
                    itemSfrPlac.Naziv = dbRead.GetString("Naziv")
                    itemSfrPlac.Vrsta = dbRead.GetString("Vrsta")
                    sfrPlacLista.Add(itemSfrPlac.Sifra, itemSfrPlac)
                Loop
                dbRead.Close()

            Catch ex As Exception
                Console.WriteLine(ex.Message)
            End Try

        End Using

        '
        ' Definiše koje su kolone R ili RW u GridDataBoundGrid tabeli
        '
        Dim iCol As Integer = 3
        cColsRW.Clear()

        Try

            For Each kvp As KeyValuePair(Of String, ClsSfrPlacItem) In sfrPlacLista

                cColsRW.Add(iCol, "R")
                If sVrstaRW.ContainsKey(kvp.Value.Vrsta) Then
                    cColsRW(iCol) = sVrstaRW(kvp.Value.Vrsta)
                    If sVrstaRW(kvp.Value.Vrsta) = "RW" AndAlso Not cSfrPlacLockException.Contains(kvp.Key) Then
                        cSfrPlacLock += kvp.Key + "|"
                    End If
                End If


                iCol += 1
            Next

            cSfrPlacLock = cSfrPlacLock.TrimEnd("|")
        Catch ex As Exception

        End Try

    End Sub

    'Public Sub SetColHeaders(ByRef pGridDataBoundGrid As Syncfusion.Windows.Forms.Grid.GridDataBoundGrid)
    '    Dim sqlText As String
    '    sqlText = <sql text="SELECT sp.id, sp.`Sifra`, sp.`Naziv`, sp.`Vrsta`  FROM `sfr_placanja` sp;"/>.Attribute("text").Value
    '    'sqlText = sqlText.Replace("?employeeID", EmployeeId)



    '    Dim dbRead As MySqlClient.MySqlDataReader

    '    pGridDataBoundGrid.GridBoundColumns.Clear()

    '    Dim gbc As New GridBoundColumn()
    '    gbc.MappingName = "EmployeeID"
    '    gbc.HeaderText = "Id"

    '    gbc.StyleInfo.BackColor = Color.FromArgb(&HC0, &HC9, &HDB)
    '    gbc.StyleInfo.TextColor = Color.Blue

    '    pGridDataBoundGrid.GridBoundColumns.Add(gbc)

    '    'Repeats for each column.
    '    gbc = New GridBoundColumn()
    '    gbc.MappingName = "Uposlenik"
    '    gbc.HeaderText = "Prezime i ime"
    '    gbc.StyleInfo.Font.Bold = True
    '    pGridDataBoundGrid.GridBoundColumns.Add(gbc)

    '    dbRead = ClsDatabaseGeneral.CreateReader(sqlText)
    '    Do While dbRead.Read
    '        gbc = New GridBoundColumn()
    '        gbc.MappingName = ClsDatabaseGeneral.DBGetText(dbRead("Sifra"))
    '        gbc.HeaderText = ClsDatabaseGeneral.DBGetText(dbRead("Naziv"))
    '        pGridDataBoundGrid.GridBoundColumns.Add(gbc)
    '        'lstSifra.Add(ClsDatabaseGeneral.DBGetText(dbRead("Sifra")).Substring(2))

    '    Loop
    '    dbRead.Close()
    '    dbRead = Nothing

    '    'Needs to initialize GridBoundColumns so their settings will replace currently set values.
    '    pGridDataBoundGrid.Binder.InitializeColumns()
    '    'Resizes column headers.
    '    'pGridDataBoundGrid.Model.ColWidths.ResizeToFit(GridRangeInfo.Row(0), GridResizeToFitOptions.NoShrinkSize)

    '    pGridDataBoundGrid.Model.Rows.HeaderCount = 1
    '    pGridDataBoundGrid.Model.RowHeights(0) = 120.0F
    '    pGridDataBoundGrid.Model.Rows.FrozenCount = 1

    '    Dim iCol As Integer = 3

    '    Try

    '        For Each kvp As KeyValuePair(Of String, ClsSfrPlacItem) In sfrPlacLista
    '            pGridDataBoundGrid.Model.Item(1, iCol).CellValue = kvp.Value.Sifra.Substring(2)
    '            iCol += 1
    '        Next
    '    Catch ex As Exception

    '    End Try

    'End Sub

    Public Function Word_Create(ByRef pDataTable As DataTable, ByVal pNazivOrgJed As String, Optional ByVal pSifraOrgJed As String = "",
                            Optional ByVal pMMGG As String = "", Optional ByVal pNazivSektora As String = "") As Syncfusion.DocIO.DLS.WordDocument

        Try


            Dim txtOrgJed As String = ""
            Dim txtParOrgJed As String = ""
            Dim txtFootOrgJed As String = ""
            Dim txtSifra As String = ""


            'conn.Close()

            ' Creating a new document.
            Dim document As New WordDocument()
            ' Adding a new section to the document.
            Dim section As IWSection = document.AddSection()
            section.PageSetup.Orientation = PageOrientation.Landscape

            ' Set Margin of the document
            section.PageSetup.Margins.All = 18
            section.PageSetup.Margins.Top = 5
            section.PageSetup.Margins.Bottom = 5

            Dim paragraph As IWParagraph = section.AddParagraph()
            'Format the heading.
            Dim text As IWTextRange
            'paragraph = section.AddParagraph()
            'Format the heading.
            ' Add a footer paragraph text to the document.
            Dim headerPar As New WParagraph(document)
            ' Add text.
            text = headerPar.AppendText("BHRT - " + pNazivSektora + " - " + pNazivOrgJed + " (" + pSifraOrgJed + ")")
            text.CharacterFormat.Bold = True
            text.CharacterFormat.FontName = "Arial"
            text.CharacterFormat.FontSize = 10.0F
            text.CharacterFormat.Italic = True
            headerPar.AppendBreak(BreakType.LineBreak)
            text = headerPar.AppendText(vbTab + vbTab + vbTab + vbTab + vbTab + vbTab + vbTab + vbTab + "Obračun plate za " + pMMGG + ". godine")
            text.CharacterFormat.Bold = True
            text.CharacterFormat.FontName = "Arial"
            text.CharacterFormat.FontSize = 12.0F
            headerPar.ParagraphFormat.HorizontalAlignment = Syncfusion.DocIO.DLS.HorizontalAlignment.Left
            headerPar.ParagraphFormat.AfterSpacing = 3.0F
            section.HeadersFooters.Header.Paragraphs.Add(headerPar)

            paragraph = section.AddParagraph()
            text = paragraph.AppendText("Napomena: Sate toplog obroka čini zbir primanja (1+8+9+10+12+14) i oni ne ulaze u ukupne mjesečne sate rada prikazane u zadnjoj koloni.")
            paragraph.ParagraphFormat.HorizontalAlignment = Syncfusion.DocIO.DLS.HorizontalAlignment.Center
            ' Tabela sati
            Try
                WordTable(section, pDataTable, "desc")

            Catch ex As Exception

            End Try


            paragraph = section.AddParagraph()
            'text = paragraph.AppendText("__________________________________(")
            text.CharacterFormat.Bold = True
            text.CharacterFormat.FontName = "Arial"
            text.CharacterFormat.FontSize = 10.0F
            text.CharacterFormat.TextColor = Color.Black
            paragraph.ParagraphFormat.BeforeSpacing = 15.0F
            paragraph.ParagraphFormat.HorizontalAlignment = Syncfusion.DocIO.DLS.HorizontalAlignment.Left

            paragraph = section.AddParagraph()
            text = paragraph.AppendText("Rukovodilac organizacione cjeline:")
            text = paragraph.AppendText("__________________________________")

            text.CharacterFormat.Bold = True
            text.CharacterFormat.FontName = "Arial"
            text.CharacterFormat.FontSize = 10.0F
            text.CharacterFormat.TextColor = Color.Black
            paragraph.ParagraphFormat.BeforeSpacing = 20.0F
            paragraph.ParagraphFormat.HorizontalAlignment = Syncfusion.DocIO.DLS.HorizontalAlignment.Left

            ' Add a footer paragraph text to the document.
            Dim footerPar As New WParagraph(document)
            ' Add text.
            text = footerPar.AppendText(txtParOrgJed & vbTab & vbTab & vbTab & "BHRT evidencijeNET-2016   " & vbTab & vbTab & " " & Now().ToShortDateString & " " & Now.ToShortTimeString)
            ' Add page and Number of pages field to the document.
            footerPar.AppendText(vbTab & vbTab & vbTab & "Strana ")
            text.CharacterFormat.Italic = True
            text.CharacterFormat.FontName = "Arial"
            text.CharacterFormat.FontSize = 10.0F
            footerPar.AppendField("Page", Syncfusion.DocIO.FieldType.FieldPage)
            footerPar.ParagraphFormat.HorizontalAlignment = Syncfusion.DocIO.DLS.HorizontalAlignment.Right
            section.HeadersFooters.Footer.Paragraphs.Add(footerPar)


            'Saving the document as .docx
            'document.Save(pNazivDoc, FormatType.Docx)
            Return document

        Catch Ex As Exception
            ' Shows the Message box with Exception message, if an exception throws.
            'MessageBoxAdv.Show(Ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.[Error])
        Finally

        End Try
    End Function

    Private Sub WordTable(ByRef section As IWSection, ByRef table As DataTable, ByVal pHeader As String)
        ' Table Word Doc
        '
        'Create a new table
        Dim textBody As WTextBody = section.Body
        Dim docTable As IWTable = textBody.AddTable()

        'Set the format for rows
        Dim format As New RowFormat()
        format.Borders.BorderType = Syncfusion.DocIO.DLS.BorderStyle.[Single]
        format.Borders.LineWidth = 1.0F
        format.Borders.Color = Color.Black

        'Initialize number of rows and cloumns.
        Dim extraRows As Integer = 2
        docTable.ResetCells(table.Rows.Count + extraRows, table.Columns.Count, format, 0)

        'Repeat the header.
        docTable.Rows(0).IsHeader = True
        docTable.Rows(1).IsHeader = True

        'Format the HEADER ROWS
        docTable.ApplyHorizontalMerge(0, 0, 1)
        docTable.ApplyHorizontalMerge(1, 0, 1)
        docTable.ApplyVerticalMerge(0, 0, 1)
        docTable.ApplyVerticalMerge(1, 0, 1)
        'Dim theadertext As IWTextRange = docTable.Rows(0).Cells(0).AddParagraph().AppendText(pHeader)

        For c As Integer = 0 To table.Columns.Count - 1

            For iRow As Integer = 0 To 1
                Dim theadertext As IWTextRange
                Select Case c
                    Case 0 To 1
                        Select Case iRow
                            Case 0
                                theadertext = docTable.Rows(0).Cells(c).AddParagraph().AppendText("Prezime i ime")
                            Case 1
                                theadertext = docTable.Rows(1).Cells(c).AddParagraph().AppendText("")
                        End Select
                    Case Else
                        Select Case iRow
                            Case 0
                                theadertext = docTable.Rows(0).Cells(c).AddParagraph().AppendText(sfrPlacLista.ElementAt(c - 2).Value.Naziv)
                            Case 1
                                theadertext = docTable.Rows(1).Cells(c).AddParagraph().AppendText(Right(sfrPlacLista.ElementAt(c - 2).Value.Sifra, 2))
                        End Select

                End Select

                theadertext.CharacterFormat.FontSize = 10.0F
                theadertext.CharacterFormat.FontName = "Arial"
                theadertext.CharacterFormat.Bold = True
                theadertext.CharacterFormat.TextColor = Color.Black

                docTable.Rows(iRow).Cells(c).CellFormat.BackColor = Color.LightGray
                docTable.Rows(iRow).Cells(c).CellFormat.Borders.Color = Color.Black
                docTable.Rows(iRow).Cells(c).CellFormat.Borders.BorderType = Syncfusion.DocIO.DLS.BorderStyle.None
                docTable.Rows(iRow).Cells(c).CellFormat.Borders.LineWidth = 1.0F

                docTable.Rows(iRow).Cells(c).CellFormat.Paddings.All = 0.0F
                docTable.Rows(iRow).Cells(c).CellFormat.TextDirection = Syncfusion.DocIO.DLS.TextDirection.Vertical
                docTable.Rows(0).Height = 80.0F
                docTable.Rows(iRow).RowFormat.HorizontalAlignment = RowAlignment.Center


                Select Case c
                    Case 0
                        docTable.Rows(iRow).Cells(c).Width = 25.0F
                    Case 1
                        docTable.Rows(iRow).Cells(c).Width = 100.0F
                    Case Else
                        docTable.Rows(iRow).Cells(c).Width = 26.0F
                End Select
            Next
            'docTable.Rows(0).Cells(c).CellFormat.BackColor = Color.White
            docTable.Rows(0).Cells(c).CellFormat.TextDirection = Syncfusion.DocIO.DLS.TextDirection.VerticalBottomToTop
            docTable.Rows(0).Cells(0).CellFormat.TextDirection = Syncfusion.DocIO.DLS.TextDirection.Horizontal
            docTable.Rows(0).Cells(0).CellFormat.VerticalAlignment = Syncfusion.DocIO.DLS.VerticalAlignment.Bottom

        Next

        'Format the table body ROWS
        For r As Integer = 0 To table.Rows.Count - 1
            For c As Integer = 0 To table.Columns.Count - 1

                Dim Value As String = table.Rows(r)(c).ToString()

                ' Ako je izraz 0 => _
                If IsNumeric(Value) Then
                    Dim numValue As Decimal = Convert.ToDecimal(Value)
                    If numValue = 0 Then Value = ""
                End If

                ' Uklanjanje decimalnog zareza i decimale 10,0 => 10
                If Right(Value, 1) = "0" Then
                    Value = Left(Value, Value.Length - 2)
                End If

                If c = 0 Then Value = (r + 1).ToString


                Dim theadertext As IWTextRange = docTable.Rows(r + extraRows).Cells(c).AddParagraph().AppendText(Value)

                theadertext.CharacterFormat.FontSize = 8.5F
                theadertext.CharacterFormat.FontName = "Arial"


                docTable.Rows(r + extraRows).Cells(c).CellFormat.BackColor = Color.White
                '
                ' Naglašavanje polja crvenom ako je preklapanje GO
                '
                'If c = 3 Then docTable.Rows(r + extraRows).Cells(c).CellFormat.BackColor = Color.Tomato

                docTable.Rows(r + extraRows).Cells(c).CellFormat.Borders.Color = Color.Black
                docTable.Rows(r + extraRows).Cells(c).CellFormat.Borders.BorderType = Syncfusion.DocIO.DLS.BorderStyle.[Single]
                docTable.Rows(r + extraRows).Cells(c).CellFormat.Borders.LineWidth = 0.5F
                docTable.Rows(r + extraRows).Cells(c).CellFormat.Paddings.All = 0.0F

                docTable.Rows(r + extraRows).Cells(c).CellFormat.VerticalAlignment = Syncfusion.DocIO.DLS.VerticalAlignment.Middle
                docTable.Rows(r + extraRows).Cells(c).CellFormat.TextDirection = Syncfusion.DocIO.DLS.TextDirection.Horizontal
                docTable.Rows(r + extraRows).RowFormat.HorizontalAlignment = RowAlignment.Right

                Select Case c
                    Case 0
                        docTable.Rows(r + extraRows).Cells(c).Width = 25.0F

                    Case 1
                        docTable.Rows(r + extraRows).Cells(c).Width = 100.0F
                    Case Else
                        docTable.Rows(r + extraRows).Cells(c).Width = 26.0F
                End Select
            Next
        Next

        ' End Word Table
    End Sub

    Public Sub HtmlTable(ByRef table As DataTable, ByRef pEmailBody As String)

        'Dim htmlBody As String = My.Resources.emailHtmlBody
        Dim htmlBody As String = ""

        Dim _tblHeader As String = ""
        For c As Integer = 0 To table.Columns.Count - 1
            Select Case c
                Case 0
                    _tblHeader += " <th class=""green"">RB</th>"
                Case 1
                    _tblHeader += " <th class=""green"">Prezime ime</th>"
                Case Else
                    '_tblHeader += " <th class=""green"">" + sfrPlacLista.ElementAt(c - 2).Value.Naziv + "<br>" +
                    'Right(sfrPlacLista.ElementAt(c - 2).Value.Sifra, 2) + "</th>"
                    _tblHeader += " <th class=""green"">" + Right(sfrPlacLista.ElementAt(c - 2).Value.Sifra, 2) + "</th>"
            End Select


        Next

        pEmailBody = pEmailBody.Replace("{tableHeader}", _tblHeader)

        Dim _tblBody As String = ""
        Dim _tblRow As String = ""
        For r As Integer = 0 To table.Rows.Count - 1

            _tblRow = ""

            For c As Integer = 0 To table.Columns.Count - 1
                Dim Value As String = table.Rows(r)(c).ToString()

                If c > 1 Then

                    ' Ako je izraz 0 => _
                    If IsNumeric(Value) Then
                        Dim numValue As Decimal = Convert.ToDecimal(Value)
                        If numValue = 0 Then Value = ""
                    End If

                    ' Uklanjanje decimalnog zareza i decimale 10,0 => 10
                    If Right(Value, 1) = "0" Then
                        Value = Left(Value, Value.Length - 2)
                    End If

                End If

                If c = 0 Then Value = (r + 1).ToString

                _tblRow += "<td class=""description"">" + Value + "</td>"
            Next
            _tblBody += "<tr>" + _tblRow + "</tr>"
        Next

        pEmailBody = pEmailBody.Replace("{tableBody}", _tblBody)

    End Sub

    Public Function getOdobriEmails(ByVal pMM As Integer, ByVal pGGGG As Integer, ByVal pUserId As Integer) As String
        Dim sqlText As String
        Dim dbResultStr As String = ""



        sqlText = <sql text="CALL `EVD_odobri_emails`('','?DbTableName','?MM','?GGGG', '?UserId' );"/>.Attribute("text").Value
        sqlText = sqlText.Replace("?DbTableName", cOrgSfr)
        sqlText = sqlText.Replace("?MM", pMM)
        sqlText = sqlText.Replace("?GGGG", pGGGG)
        sqlText = sqlText.Replace("?UserId", pUserId)

        Try

            Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
                myconnection.Open()

                Dim mycmd As New MySql.Data.MySqlClient.MySqlCommand
                Dim dbRead As MySqlClient.MySqlDataReader

                mycmd.Connection = myconnection

                mycmd.CommandText = sqlText


                Try
                    dbRead = mycmd.ExecuteReader
                    Do While dbRead.Read
                        dbResultStr = dbRead.GetString("emails")
                    Loop
                    dbRead.Close()

                Catch ex As Exception
                    Console.WriteLine(ex.Message)
                End Try

            End Using


        Catch ex As Exception
            Dim el As New ErrorsAndEvents.ErrorLogger
            el.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try

        Return dbResultStr
    End Function
End Class

Public Class ClsSfrPlacItem

    Public UniqueId As Integer

    Public Sifra As String
    Public Naziv As String
    Public Vrsta As String

    Public Overrides Function ToString() As String
        Return Naziv + " - " + Sifra.Substring(2, Sifra.Length - 2)
    End Function
End Class



