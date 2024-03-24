Imports Microsoft.VisualBasic

Public Class ngPnPutniNalog

    Public Property id As Integer
    Public Property employee_id As Integer
    Public Property ime_prezime As String
    Public Property radno_mjesto As String
    Public Property uposlenik As Integer
    Public Property mjesto_start As String
    Public Property mjesto_putovanja As String
    Public Property razlog_putovanja As String
    Public Property dat_poc_putovanja As Date
    Public Property trajanje_putovanja As Decimal
    Public Property relacija As String
    Public Property prev_text As String
    Public Property PrevAuto As Boolean
    Public Property PrevVlAuto As Boolean
    Public Property PrevAvion As Boolean
    Public Property PrevAutobus As Boolean
    Public Property PrevVoz As Boolean
    Public Property troskovi_putovanja_knjizenje As String
    Public Property aktivnost As String
    Public Property org_jed As String
    Public Property UkTroskoviA As Decimal
    Public Property UkTroskoviB As Decimal
    Public Property UkTroskoviC As Decimal
    Public Property UkTroskoviD As Decimal
    Public Property UkIznosObracuna As Decimal
    Public Property UkIznosRazlika As Decimal
    Public Property iznos_dnevnice As Decimal
    Public Property proc_dnevnice As Decimal
    Public Property broj_dnevnica As Decimal
    Public Property akontacija_dnevnice As Decimal
    Public Property akontacija_nocenje As Decimal
    Public Property akontacija_ostalo As Decimal
    Public Property iznos_akontacije As Decimal
    Public Property ispl_akontacije As Decimal
    Public Property odobrena_akontacija As Boolean
    Public Property vrsta_naloga As Integer
    Public Property status_naloga As String
    Public Property status_date As Date
    Public Property created_by As String
    Public Property potpis_naloga As String
    Public Property isSignee As Integer
    Public Property nazivorgjed As String
    Public Property izvjestaj As String
    Public Property temeljnica_1 As String  ' HACK: temelnjica koji upisuje blagajna      
    Public Property temeljnica_2 As String
    Public Property pn_protokol As String
End Class
