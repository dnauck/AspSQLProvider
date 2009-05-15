<%@ Page Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true"
	CodeFile="Login.aspx.cs" Inherits="Login" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContentPlaceHolder" runat="Server">
	Login
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder" runat="Server">
	<asp:Login ID="Login1" runat="server" CreateUserText="Sign Up for Your New Account" CreateUserUrl="~/Register.aspx" DestinationPageUrl="~/Account/Profile.aspx" TitleText="">
	</asp:Login>
</asp:Content>
