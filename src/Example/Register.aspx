<%@ Page Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true"
	CodeFile="Register.aspx.cs" Inherits="Register" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContentPlaceHolder" runat="Server">
	Sign Up for Your New Account
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder" runat="Server">
	<asp:CreateUserWizard ID="CreateUserWizard1" runat="server" ContinueDestinationPageUrl="~/Account/Profile.aspx">
		<WizardSteps>
			<asp:CreateUserWizardStep ID="CreateUserWizardStep1" runat="server" Title="">
			</asp:CreateUserWizardStep>
			<asp:CompleteWizardStep ID="CompleteWizardStep1" runat="server">
			</asp:CompleteWizardStep>
		</WizardSteps>
	</asp:CreateUserWizard>
</asp:Content>
