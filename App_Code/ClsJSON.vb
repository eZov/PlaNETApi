Imports Newtonsoft.Json
Imports System.IO
Imports Newtonsoft.Json.Linq
Imports System.Data

Public Class ClsJSON

    Public dataSet2 As DataSet


    Public Shared Function getItemList(ByVal pJSON As String) As List(Of ClsMenu)

        Dim _mniItemsList As IList(Of ClsMenu) = New List(Of ClsMenu)()

        Dim reader As New JsonTextReader(New StringReader(pJSON))
        reader.SupportMultipleContent = True


        Try
            While True
                If Not reader.Read() Then
                    Exit While
                End If

                Dim serializer As New JsonSerializer()
                Dim _mnuItem As ClsMenu = serializer.Deserialize(Of ClsMenu)(reader)
                If _mnuItem.ParentId = 0 Then _mnuItem.ParentId = Nothing
                '_mnuItem.Link = "Default.aspx"
                _mniItemsList.Add(_mnuItem)


            End While

            Return _mniItemsList
        Catch ex As Exception
            Dim el As New ErrorsAndEvents.ErrorLogger
            el.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try

        Return Nothing

    End Function


    Public Shared Function GetJSON(ByVal table As DataTable) As String
        Dim jsonString As String = String.Empty
        jsonString = JsonConvert.SerializeObject(table)
        Return jsonString
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="jsonOpis"></param>
    ''' <returns>DataTable</returns>
    ''' <remarks></remarks>
    Public Shared Function GetDataTable(ByVal jsonOpis As String) As DataTable

        Dim jsonDataTable As DataTable = Nothing
        Try
            jsonDataTable = JsonConvert.DeserializeObject(Of DataTable)(jsonOpis)
        Catch ex As Exception

        End Try

        Return jsonDataTable
    End Function

    Public Shared Function GetDataSet(ByVal jsonOpis As String) As DataSet

        Dim jsonDataSet As DataSet = Nothing
        jsonDataSet = JsonConvert.DeserializeObject(Of DataSet)(jsonOpis)
        Return jsonDataSet
    End Function
End Class

Public Class Product
    Public Name As String
    Public ExpiryDate As DateTime
    Public Price As Decimal
    Public Sizes() As String
End Class


