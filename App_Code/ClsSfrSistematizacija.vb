Imports System.Data
Imports Microsoft.VisualBasic
Imports MySql.Data
Imports MySql.Data.MySqlClient

Public Class ClsSfrSistematizacija

    Public Sub New()

        DtList = New DataTable

    End Sub

    Public Property LboxList As New List(Of ClsSistematizacija)
    Public Property DtList As New DataTable

    Public Property DtRow As DataRow

    Public Function getList(Optional ByVal pId As Integer = -1) As DataTable

        Dim myda As MySqlClient.MySqlDataAdapter
        Dim mycmd As MySqlCommand

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim strSQLWhere As String = ""
            If pId <> -1 Then
                strSQLWhere = String.Format("WHERE o.id={0}", pId.ToString)
            End If


            mycmd = New MySqlCommand
            mycmd.Connection = myconnection

            Dim strSQL As String = <![CDATA[ 
SELECT o.id AS Id, o.Sifra, o.Naziv, o.Opis, o.StrSprema_1, o.id AS hidId, 0 AS ParentId,
0 AS HasChild, NULL AS Expanded
FROM sfr_sistematizacija o
%_WHERE_% 
 ORDER BY  o.Naziv;
]]>.Value
            strSQL = strSQL.Replace("%_WHERE_%", strSQLWhere)


            DtList.Rows.Clear()
            myda = New MySqlClient.MySqlDataAdapter(strSQL, myconnection)
            myda.Fill(DtList)

        End Using


        LboxList.Clear()

        For Each er As DataRow In DtList.Rows
            LboxList.Add(New ClsSistematizacija With {.Id = er.Item("id"), .Naziv = er.Item("Naziv") + "(" + er.Item("Sifra") + ")"})
        Next

        Return DtList

    End Function

    Public Function getIdx(ByVal pId As Integer) As Integer

        Dim idx As Integer = -1

        For Each er In DtList.Rows
            idx += 1
            If er("id") = pId Then
                Return idx
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
                retVal = CInt(er("id"))
                Return retVal
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
UPDATE sfr_sistematizacija
SET
 Sifra = @Sifra,
 Naziv = @Naziv, 
 Opis = @Opis,
 StrSprema_1 = @StrSprema_1

WHERE id = @id;
    ]]>.Value


                mycmd.CommandText = strSQL
    

                mycmd.Parameters.AddWithValue("@id", pDbFields("id"))
                mycmd.Parameters.AddWithValue("@Sifra", pDbFields("Sifra"))

                mycmd.Parameters.AddWithValue("@Naziv", pDbFields("Naziv"))
                mycmd.Parameters.AddWithValue("@Opis", pDbFields("Opis"))

                mycmd.Parameters.AddWithValue("@StrSprema_1", pDbFields("StrSprema_1"))

                mycmd.Prepare()

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
INSERT INTO sfr_sistematizacija (
Sifra,  Naziv, Opis, StrSprema_1 
)
VALUES
  (
   @Sifra, @Naziv, @Opis, @StrSprema_1
  );
    ]]>.Value

                mycmd.CommandText = strSQL
    
                'mycmd.Parameters.AddWithValue("@id", pDbFields("id"))
                mycmd.Parameters.AddWithValue("@Sifra", pDbFields("Sifra"))

                mycmd.Parameters.AddWithValue("@Naziv", pDbFields("Naziv"))
                mycmd.Parameters.AddWithValue("@Opis", pDbFields("Opis"))

                mycmd.Parameters.AddWithValue("@StrSprema_1", pDbFields("StrSprema_1"))

                mycmd.Prepare()

                mycmd.ExecuteNonQuery()

                pRowId = mycmd.LastInsertedId

                '
                '   Prepiši Id u Sifra
                '
                pDbFields("Sifra") = pRowId.ToString
                pDbFields("id") = pRowId

                saveRow(pDbFields)
            Catch ex As Exception
                ex.ToString()
            End Try


        End Using


    End Sub


    Public Function rowDetails(ByVal pId As Integer, ByVal pColName As String) As Object

        If pId < 0 Or pColName.Length = 0 Then Return Nothing

        For Each er In DtList.Rows
            If er("id") = pId Then
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
