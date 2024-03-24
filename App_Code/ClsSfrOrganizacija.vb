Imports System.Data
Imports Microsoft.VisualBasic
Imports MySql.Data
Imports MySql.Data.MySqlClient

Public Class ClsSfrOrganizacija


    Public Sub New()

    End Sub

    Public Property LboxList As New List(Of ClsTreeView)
    Public Property DtList As New DataTable
    Public Property DtRow As DataRow
    Private Property strSQLWhere As String = ""

    Public Sub setObracun()
        strSQLWhere = " WHERE Obracun <> 0 AND Atribut_1='D'"
    End Sub

    Public Sub setUposlenik()
        strSQLWhere = " WHERE Uposlenici <> 0 AND Atribut_1='D'"
    End Sub

    Public Sub setOrganizacija()
        strSQLWhere = " WHERE o.`Sifra` IN('102')"
    End Sub

    'TODO Razmotriti gdje proziva id reda a gdje Sifra // korigovan Sql upit da pretraži Sifra
    ''' <summary>
    ''' pSfrId je Sifra, a hidId je Id
    ''' </summary>
    ''' <returns></returns>
    Public Function getList(Optional ByVal pSfrId As Integer = -1) As DataTable

        Dim myda As MySqlClient.MySqlDataAdapter
        Dim mycmd As MySqlCommand
        Dim _SfrOrgJed As String = " "
        Dim _SfrOrgJedSel As String = " "

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            'Dim strSQLWhere As String = ""
            If pSfrId <> -1 Then
                'strSQLWhere = String.Format("WHERE o.id={0}", pId.ToString)
            End If


            Dim strSQL As String = <![CDATA[ 
SELECT o.Sifra AS Id, IF(o.Sifra=o.`Sifra Nadnivo`,'0',o.`Sifra Nadnivo`) AS ParentId, 
CONCAT(o.Naziv,' (',o.`Sifra`,')') AS Naziv, 1 AS HasChild, NULL AS Expanded, 
Opcina_1,  o.`Sifra Nadnivo` AS SfrNadNivo, o.Obracun, o.Uposlenici, o.Atribut_1, o.Naziv AS Naziv1, o.Opis,
Mjesto, Adresa, PTT_Broj,
BrojDjelovodni, BrojStatisticki, PDVBroj,
Banka_1, Banka_2, BankaRacun_1,  BankaRacun_2,
o.id AS hidId, o.Naziv AS oNaziv, o.Rukovodilac AS Selected
FROM sfr_organizacija o
%_WHERE_% 
ORDER BY o.Sifra;
]]>.Value
            If strSQLWhere.Length = 0 Then
                strSQL = strSQL.Replace("%_WHERE_%", " WHERE o.Atribut_1='D'")
            Else
                strSQL = strSQL.Replace("%_WHERE_%", strSQLWhere)
            End If


            DtList.Clear()
            myda = New MySqlClient.MySqlDataAdapter(strSQL, myconnection)
            myda.Fill(DtList)

            '
            ' Ako je tabela prazna, izađi...
            '
            If DtList.Rows.Count = 0 Then Return DtList


            '
            ' Ako je pId = -1 odaberi top red iz tabele
            '
            If pSfrId = -1 Then

                Dim filteredTableRow() As DataRow = DtList.Select("ParentId=0")
                For Each er In filteredTableRow
                    pSfrId = er("hidId")
                    Exit For
                Next

            End If

            '
            ' Procesing Expand i Select node...
            '
            mycmd = New MySqlCommand
            mycmd.Connection = myconnection

            strSQL = <![CDATA[ 
SELECT so.`Sifra`, CONCAT(so1.`Sifra`,',', so2.`Sifra`,',', so3.`Sifra`,',',  so4.`Sifra` ,',',  so5.`Sifra`) AS sf  FROM `sfr_organizacija` so 
LEFT JOIN `sfr_organizacija` so1 ON so.`Sifra Nadnivo`=so1.`Sifra`
LEFT JOIN `sfr_organizacija` so2 ON so1.`Sifra Nadnivo`=so2.`Sifra`
LEFT JOIN `sfr_organizacija` so3 ON so2.`Sifra Nadnivo`=so3.`Sifra`
LEFT JOIN `sfr_organizacija` so4 ON so3.`Sifra Nadnivo`=so4.`Sifra`
LEFT JOIN `sfr_organizacija` so5 ON so4.`Sifra Nadnivo`=so5.`Sifra`
WHERE  so.Sifra=@Id AND so.Atribut_1='D' AND so1.Atribut_1='D' AND so2.Atribut_1='D' AND so3.Atribut_1='D' AND so4.Atribut_1='D';
]]>.Value
            '
            ' TODO razmotriti  so.id=@Id promijenjeno u so.Sifra
            '
            mycmd.CommandText = strSQL


            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@Id", pSfrId)
            mycmd.Prepare()

            Dim rd As MySqlDataReader = mycmd.ExecuteReader()

            While rd.Read
                If rd.IsDBNull(rd.GetOrdinal("sf")) = False Then
                    _SfrOrgJed = rd.GetString(rd.GetOrdinal("sf"))
                End If
                _SfrOrgJedSel = rd.GetString("Sifra")
            End While

            rd.Close()

        End Using


        PopulateLboxList(_SfrOrgJed, _SfrOrgJedSel)

        Return DtList

    End Function

    Public Function getListByEmpId(ByVal pSessionId As String,
                                   ByVal pEmployeeID As Integer, Optional ByVal pSfrId As Integer = -1) As DataTable

        Dim myda As MySqlClient.MySqlDataAdapter
        Dim mycmd As MySqlCommand
        Dim _SfrOrgJed As String = " "
        Dim _SfrOrgJedSel As String = " "

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            mycmd = New MySqlCommand
            mycmd.Connection = myconnection


            Dim strSQL As String = <![CDATA[ 
CALL EVDwb_orglist(@SessionId, @EmpId);
]]>.Value

            mycmd.CommandText = strSQL


            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@SessionId", pSessionId)
            mycmd.Parameters.AddWithValue("@EmpId", pEmployeeID)
            mycmd.Prepare()

            myda = New MySqlClient.MySqlDataAdapter(mycmd)

            DtList.Clear()
            'myda = New MySqlClient.MySqlDataAdapter(strSQL, myconnection)
            myda.Fill(DtList)

            '
            ' Ako je tabela prazna, izađi...
            '
            If DtList.Rows.Count = 0 Then Return DtList


            '
            ' Ako je pId = -1 odaberi top red iz tabele
            '
            If pSfrId = -1 Then

                Dim filteredTableRow() As DataRow = DtList.Select("ParentId=0")
                For Each er In filteredTableRow
                    pSfrId = er("hidId")
                    Exit For
                Next

            End If

            '
            ' Procesing Expand i Select node...
            '
            mycmd = New MySqlCommand
            mycmd.Connection = myconnection

            strSQL = <![CDATA[ 
SELECT so.`Sifra`, CONCAT(so1.`Sifra`,',', so2.`Sifra`,',', so3.`Sifra`,',',  so4.`Sifra` ,',',  so5.`Sifra`) AS sf  FROM `sfr_organizacija` so 
LEFT JOIN `sfr_organizacija` so1 ON so.`Sifra Nadnivo`=so1.`Sifra`
LEFT JOIN `sfr_organizacija` so2 ON so1.`Sifra Nadnivo`=so2.`Sifra`
LEFT JOIN `sfr_organizacija` so3 ON so2.`Sifra Nadnivo`=so3.`Sifra`
LEFT JOIN `sfr_organizacija` so4 ON so3.`Sifra Nadnivo`=so4.`Sifra`
LEFT JOIN `sfr_organizacija` so5 ON so4.`Sifra Nadnivo`=so5.`Sifra`
WHERE so.id=@Id AND so.Atribut_1='D' AND so1.Atribut_1='D' AND so2.Atribut_1='D' AND so3.Atribut_1='D' AND so4.Atribut_1='D';
]]>.Value
            mycmd.CommandText = strSQL


            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@Id", pSfrId)
            mycmd.Prepare()

            Dim rd As MySqlDataReader = mycmd.ExecuteReader()

            While rd.Read
                If rd.IsDBNull(rd.GetOrdinal("sf")) = False Then
                    _SfrOrgJed = rd.GetString(rd.GetOrdinal("sf"))
                End If
                _SfrOrgJedSel = rd.GetString("Sifra")
            End While

            rd.Close()

        End Using


        PopulateLboxList(_SfrOrgJed, _SfrOrgJedSel)

        Return DtList

    End Function

    ''' <summary>
    ''' Expanduje tree liniju gdje se nalazi node koji je SELECTED
    ''' </summary>
    ''' <param name="pSfrOrgJed"></param>
    ''' <param name="pSfrOrgJedSel"></param>
    Private Sub PopulateLboxList(ByVal pSfrOrgJed As String, ByVal pSfrOrgJedSel As String)

        LboxList.Clear()

        Dim _parentId As Integer = 0

        For Each er As DataRow In DtList.Rows
            If er.Item("ParentId") = "" Then
                _parentId = 0
            Else
                _parentId = CInt(er.Item("ParentId"))
            End If

            Dim _SfrOrgJedLoopInt As Integer = -1
            Integer.TryParse(er.Item("id"), _SfrOrgJedLoopInt)
            '
            ' Ako je <=0, excepetion 
            '
            If _SfrOrgJedLoopInt <= 0 Then Exit For

            Dim _SfrOrgJedLoop As String = _SfrOrgJedLoopInt.ToString

            If pSfrOrgJed.Contains(_SfrOrgJedLoop) And pSfrOrgJedSel <> _SfrOrgJedLoop Then
                LboxList.Add(New ClsTreeView With {.Id = er.Item("id"), .ParentId = _parentId, .Naziv = er.Item("Naziv"), .HasChild = "true", .Expanded = True, .Selected = False})
            ElseIf pSfrOrgJedSel = _SfrOrgJedLoop Then
                LboxList.Add(New ClsTreeView With {.Id = er.Item("id"), .ParentId = _parentId, .Naziv = er.Item("Naziv"), .HasChild = "true", .Expanded = False, .Selected = True})
            Else
                LboxList.Add(New ClsTreeView With {.Id = er.Item("id"), .ParentId = _parentId, .Naziv = er.Item("Naziv"), .HasChild = "true", .Expanded = False, .Selected = False})
            End If

        Next

    End Sub

    ''' <summary>
    ''' Lista samo onih koji imaju obračun
    ''' Id je Sifra, a hidId je Id
    ''' </summary>
    ''' <returns></returns>
    Public Function getListObr() As DataTable

        Dim myda As MySqlClient.MySqlDataAdapter

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim strSQL As String = <![CDATA[ 
SELECT o.Sifra AS Id, IF(o.Sifra=o.`Sifra Nadnivo`,'0',o.`Sifra Nadnivo`) AS ParentId, 
CONCAT(o.Naziv,' (',o.`Sifra`,')') AS Naziv, 1 AS HasChild, NULL AS Expanded, 
Opcina_1,  o.`Sifra Nadnivo` AS SfrNadNivo, o.Obracun, o.Uposlenici, o.Naziv AS Naziv1, o.Opis,
Mjesto, Adresa, PTT_Broj,
BrojDjelovodni, BrojStatisticki, PDVBroj,
Banka_1, Banka_2, BankaRacun_1,  BankaRacun_2,
o.id AS hidId, o.Naziv AS oNaziv
FROM sfr_organizacija o
WHERE Obracun<>0
ORDER BY o.Sifra;
]]>.Value

            DtList.Clear()
            myda = New MySqlClient.MySqlDataAdapter(strSQL, myconnection)
            myda.Fill(DtList)

        End Using

        Return DtList

    End Function


    Public Function getIdx(Optional ByVal pSfrOrganizacija As String = "000000") As Integer

        If pSfrOrganizacija.Length = 0 Then Return -1
        Dim idx As Integer = -1

        For Each er In DtList.Rows

            idx += 1
            If er("Id") = pSfrOrganizacija Then
                Exit For
            End If
        Next

        Return idx
    End Function

    Public Function getIdx(ByVal pIdOrganizacija As Integer) As Integer

        If pIdOrganizacija <= 0 Then Return -1
        Dim idx As Integer = -1

        For Each er In DtList.Rows

            idx += 1
            If er("hidId") = pIdOrganizacija Then
                Return idx
                'Exit For
            End If
        Next

        Return -1
    End Function

    Public Function getSifra(ByVal pIdx As Integer) As String

        Dim idx As Integer = -1
        Dim retVal As String = ""

        For Each er In DtList.Rows
            idx += 1
            If idx = pIdx Then
                retVal = er("Id")
                Return retVal
                'Exit For
            End If
        Next

        Return ""
    End Function

    Public Function getId(ByVal pIdx As Integer) As Integer

        Dim idx As Integer = -1
        Dim retVal As Integer = -1

        For Each er In DtList.Rows
            idx += 1
            If idx = pIdx Then
                retVal = CInt(er("hidId"))
                Return retVal
                'Exit For
            End If
        Next

        Return -1
    End Function

    Public Sub saveRow(ByRef pDbFields As Hashtable)


        Dim mycmd As MySqlCommand

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Try
                mycmd = New MySqlCommand
                mycmd.Connection = myconnection

                Dim strSQL As String = <![CDATA[ 
UPDATE sfr_organizacija
SET
 Sifra = @Sifra, `Sifra Nadnivo` = @Sifra_Nadnivo, Naziv = @Naziv, Opis = @Opis,
 Opcina_1 = @Opcina_1,
 Banka_1 = @Banka_1, Banka_2 = @Banka_2,
 Obracun = @Obracun, Uposlenici = @Uposlenici,      
  Atribut_1 = @Atribut_1,      
  BrojDjelovodni = @BrojDjelovodni, 
  BrojStatisticki = @BrojStatisticki,  
  BankaRacun_1 = @BankaRacun_1, BankaRacun_2 = @BankaRacun_2, 
  Mjesto = @Mjesto, Adresa = @Adresa, PTT_Broj = @PTT_Broj, 
  PDVBroj = @PDVBroj

WHERE id = @id;
    ]]>.Value


                mycmd.CommandText = strSQL
    

                mycmd.Parameters.AddWithValue("@id", pDbFields("id"))
                mycmd.Parameters.AddWithValue("@Sifra", pDbFields("Sifra"))
                mycmd.Parameters.AddWithValue("@Sifra_Nadnivo", pDbFields("Sifra_Nadnivo"))

                mycmd.Parameters.AddWithValue("@Naziv", pDbFields("Naziv"))
                mycmd.Parameters.AddWithValue("@Opis", pDbFields("Opis"))

                mycmd.Parameters.AddWithValue("@Opcina_1", pDbFields("Opcina_1"))
                mycmd.Parameters.AddWithValue("@Banka_1", pDbFields("Banka_1"))
                mycmd.Parameters.AddWithValue("@Banka_2", pDbFields("Banka_2"))

                mycmd.Prepare()

                If pDbFields("Sifra") = pDbFields("Sifra_Nadnivo") Then
                    pDbFields("Obracun") = 1
                Else
                    pDbFields("Obracun") = 0
                End If

                If pDbFields("Aktivan") <> "D" Then
                    pDbFields("Obracun") = 0
                    pDbFields("Uposlenici") = 0
                End If

                mycmd.Parameters.AddWithValue("@Obracun", pDbFields("Obracun"))
                mycmd.Parameters.AddWithValue("@Uposlenici", pDbFields("Uposlenici"))
                mycmd.Parameters.AddWithValue("@@Atribut_1", pDbFields("Aktivan"))

                mycmd.Parameters.AddWithValue("@PDVBroj", pDbFields("PDVBroj"))
                mycmd.Parameters.AddWithValue("@BrojStatisticki", pDbFields("BrojStatisticki"))
                mycmd.Parameters.AddWithValue("@BrojDjelovodni", pDbFields("BrojDjelovodni"))

                mycmd.Parameters.AddWithValue("@BankaRacun_1", pDbFields("BankaRacun_1"))
                mycmd.Parameters.AddWithValue("@BankaRacun_2", pDbFields("BankaRacun_2"))

                mycmd.Parameters.AddWithValue("@Adresa", pDbFields("Adresa"))
                mycmd.Parameters.AddWithValue("@Mjesto", pDbFields("Mjesto"))
                mycmd.Parameters.AddWithValue("@PTT_Broj", pDbFields("PTT_Broj"))




                mycmd.ExecuteNonQuery()
            Catch ex As Exception
                ex.ToString()
            End Try


        End Using

    End Sub

    Public Sub insRow(ByRef pDbFields As Hashtable, Optional ByRef pRowId As Integer = -1)


        Dim mycmd As MySqlCommand

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Try
                mycmd = New MySqlCommand
                mycmd.Connection = myconnection

                Dim strSQL As String = <![CDATA[ 
INSERT INTO sfr_organizacija (
 Sifra, `Sifra Nadnivo`, Naziv, Opis, Opcina_1, Banka_1, Banka_2, 
 Obracun, Uposlenici, BrojDjelovodni, BrojStatisticki, 
 BankaRacun_1, BankaRacun_2, Mjesto, Adresa, PTT_Broj, PDVBroj
)
VALUES
  (
   @Sifra, @Sifra_Nadnivo, @Naziv, @Opis, @Opcina_1, @Banka_1, @Banka_2, 
   @Obracun, @Uposlenici, @BrojDjelovodni, @BrojStatisticki, 
   @BankaRacun_1, @BankaRacun_2, @Mjesto, @Adresa, @PTT_Broj, @PDVBroj
  );
    ]]>.Value


                mycmd.CommandText = strSQL
    

                'mycmd.Parameters.AddWithValue("@id", pDbFields("id"))
                mycmd.Parameters.AddWithValue("@Sifra", pDbFields("Sifra"))
                mycmd.Parameters.AddWithValue("@Sifra_Nadnivo", pDbFields("Sifra_Nadnivo"))

                mycmd.Parameters.AddWithValue("@Naziv", pDbFields("Naziv"))
                mycmd.Parameters.AddWithValue("@Opis", pDbFields("Opis"))

                mycmd.Parameters.AddWithValue("@Opcina_1", pDbFields("Opcina_1"))
                mycmd.Parameters.AddWithValue("@Banka_1", pDbFields("Banka_1"))
                mycmd.Parameters.AddWithValue("@Banka_2", pDbFields("Banka_2"))

                mycmd.Parameters.AddWithValue("@Obracun", pDbFields("Obracun"))
                mycmd.Parameters.AddWithValue("@Uposlenici", pDbFields("Uposlenici"))

                mycmd.Parameters.AddWithValue("@PDVBroj", pDbFields("PDVBroj"))
                mycmd.Parameters.AddWithValue("@BrojStatisticki", pDbFields("BrojStatisticki"))
                mycmd.Parameters.AddWithValue("@BrojDjelovodni", pDbFields("BrojDjelovodni"))

                mycmd.Parameters.AddWithValue("@BankaRacun_1", pDbFields("BankaRacun_1"))
                mycmd.Parameters.AddWithValue("@BankaRacun_2", pDbFields("BankaRacun_2"))

                mycmd.Parameters.AddWithValue("@Adresa", pDbFields("Adresa"))
                mycmd.Parameters.AddWithValue("@Mjesto", pDbFields("Mjesto"))
                mycmd.Parameters.AddWithValue("@PTT_Broj", pDbFields("PTT_Broj"))

                mycmd.Prepare()

                mycmd.ExecuteNonQuery()

                pRowId = mycmd.LastInsertedId
                '
                '   Prepiši Id u Sifra
                '
                pDbFields("Sifra") = pRowId.ToString
                pDbFields("id") = pRowId

                If CInt(pDbFields("Sifra_Nadnivo")) < 0 Then
                    pDbFields("Sifra_Nadnivo") = pRowId.ToString
                End If

                saveRow(pDbFields)

            Catch ex As Exception
                ex.ToString()
            End Try


        End Using


    End Sub

    ''' <summary>
    ''' Vraća vrijednost u tabeli za red i kolonu
    ''' </summary>
    ''' <param name="pId"></param>
    ''' <param name="pColName"></param>
    ''' <returns></returns>
    Public Function rowDetails(ByVal pId As Integer, ByVal pColName As String) As Object

        If pId < 0 Or pColName.Length = 0 Then Return Nothing

        For Each er In DtList.Rows
            If er("hidId") = pId Then
                DtRow = er
                Exit For
            End If
        Next

        If DtRow IsNot Nothing Then
            Return DtRow.Item(pColName)
        Else
            Return Nothing
        End If

    End Function


End Class
