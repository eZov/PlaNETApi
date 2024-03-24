Imports System.Data
Imports Microsoft.VisualBasic

Public Class ClsHtml
    Dim cHtmlTmpl As String

    Sub New()

        cHtmlTmpl = <![CDATA[ 
<html lang="en">
    <head>    
        <meta content="text/html; charset=utf-8" http-equiv="Content-Type">
        <title>
            Dnevnik rada
        </title>
        <style type="text/css">
            HTML{background-color: #e8e8e8;}
            .courses-table{font-size: 12px; padding: 2px; border-collapse: collapse; border-spacing: 0;}
            .courses-table .description{color: #505050;}
            .courses-table td{border: 1px solid #D1D1D1; background-color: #F3F3F3; padding: 0 2px;}
            .courses-table th{border: 1px solid #424242; color: #FFFFFF;text-align: left; padding: 0 2px;width:40px;}
            .green{background-color: #6B9852;}
         td.opis {
            width:150px; 
         }
        </style>
    </head>
    <body>
        <table class="courses-table" width="600">
            <thead>
                <tr>
					{tableHeader}
                </tr>
            </thead>
            <tbody>
					{tableBody}
            </tbody>
        </table>
    </body>
</html>
]]>.Value

    End Sub

    Public Sub HtmlTable(ByRef table As DataTable, ByRef pEmailBody As String)

        Dim htmlBody As String = cHtmlTmpl
        pEmailBody = cHtmlTmpl


        Dim _tblHeader As String = <![CDATA[ 
                                      <th class="green">ID</th>
                                      <th class="green">Dan</th>                                      
                                      <th class="green">Od</th>
                                      <th class="green">Do</th>
                                      <th class="green">Opis rada</th>                                      
                                      <th class="green">Vrsta odsustva</th>    
]]>.Value


        pEmailBody = pEmailBody.Replace("{tableHeader}", _tblHeader)

        Dim _tblBody As String = ""
        Dim _tblRow As String = ""
        For r As Integer = 0 To table.Rows.Count - 1

            _tblRow = ""

            'For c As Integer = 0 To table.Columns.Count - 1
            '    Dim Value As String = table.Rows(r)(c).ToString()

            '    If c > 1 Then

            '        ' Ako je izraz 0 => _
            '        If IsNumeric(Value) Then
            '            Dim numValue As Decimal = Convert.ToDecimal(Value)
            '            If numValue = 0 Then Value = ""
            '        End If

            '        ' Uklanjanje decimalnog zareza i decimale 10,0 => 10
            '        If Right(Value, 1) = "0" Then
            '            Value = Left(Value, Value.Length - 2)
            '        End If

            '    End If

            '    If c = 0 Then Value = (r + 1).ToString

            '    _tblRow += "<td class=""description"">" + Value + "</td>"
            'Next

            Dim pId As String
            Dim pOdsustvo As String = ""

            If table.Rows(r).Item("ProgramId") >= 0 Then
                pId = table.Rows(r).Item("ProgramId").ToString()
            Else
                pId = ""
            End If

            Select Case table.Rows(r).Item("Comments").ToString()
                Case "GO"
                    pOdsustvo = "Godišnji odmor"
                Case "PLC"
                    pOdsustvo = "Plaćeno"
                Case "NPL"
                    pOdsustvo = "Neplaćeno"
                Case "SND"
                    pOdsustvo = "Slobodno"
                Case "BOL"
                    pOdsustvo = "Bolovanje"
                Case "POD"
                    pOdsustvo = "Porodiljsko"
                Case "PRV"
                    pOdsustvo = "Vjerski praznik"
                Case "SP"
                    pOdsustvo = "Službeni put"
                Case Else
                    pOdsustvo = ""
            End Select

            Dim pST, pET As String

            If table.Rows(r).Item("ProgramStartTime").ToString() = "00:00" Then
                pST = ""
            Else
                pST = table.Rows(r).Item("ProgramStartTime").ToString()
            End If

            If table.Rows(r).Item("ProgramEndTime").ToString() = "00:00" Then
                pET = ""
            Else
                pET = table.Rows(r).Item("ProgramEndTime").ToString()
            End If

            Try
                _tblRow = "<td class=""description"">" + pId + "</td>" + _
"<td class=""description"">" + table.Rows(r).Item("schedDay").ToString() + "</td>" + _
"<td class=""description"">" + pST + "</td>" + _
"<td class=""description"">" + pET + "</td>" + _
"<td class=""opis"" width=""250"">" + table.Rows(r).Item("ProgramName") + "</td>" + _
"<td class=""description"">" + pOdsustvo + "</td>"

            Catch ex As Exception

            End Try



            _tblBody += "<tr>" + _tblRow + "</tr>"
        Next

        pEmailBody = pEmailBody.Replace("{tableBody}", _tblBody)

    End Sub
End Class
