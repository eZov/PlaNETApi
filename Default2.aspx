<%@ Page Title="" Language="VB" MasterPageFile="~/Site.master" AutoEventWireup="false" CodeFile="Default2.aspx.vb" Inherits="Default2" %>

<asp:Content ID="Content1" ContentPlaceHolderID="FeaturedContent" runat="Server">
    FeaturedContent
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="Server">
    <div id="tv_navigation" class="left-div">
        <ej:TreeView ID="TreeView1" runat="server" EnableViewState="False" EnablePersistence="True" FullRowSelect="True" Height="720px"></ej:TreeView>
    </div>
    <div id="data_form" class="right-div formTab formContent">
    </div>
</asp:Content>

