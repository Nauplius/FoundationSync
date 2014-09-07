<%@ Assembly Name="$SharePoint.Project.AssemblyFullName$" %>
<%@ Import Namespace="Microsoft.SharePoint.ApplicationPages" %>
<%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" 
    Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" 
    Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Import Namespace="Microsoft.SharePoint" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ProfileSettings.aspx.cs" Inherits="Nauplius.SP.UserSync.ADMIN.FoundationSync.ProfileSettings" 
    DynamicMasterPageFile="~masterurl/default.master" %>

<%@ Register TagPrefix="wssuc" TagName="InputFormSection" Src="~/_controltemplates/InputFormSection.ascx" %>
<%@ Register TagPrefix="wssuc" TagName="InputFormControl" Src="~/_controltemplates/InputFormControl.ascx" %>
<%@ Register TagPrefix="wssuc" TagName="ButtonSection" Src="~/_controltemplates/ButtonSection.ascx" %>

<asp:Content ID="PageHead" ContentPlaceHolderID="PlaceHolderAdditionalPageHead" runat="server">
</asp:Content>

<asp:Content ID="Main" ContentPlaceHolderID="PlaceHolderMain" runat="server">
    <table border="0" cellspacing="0" cellpadding="0" class="ms-propertysheet" width="100%">
        <wssuc:buttonsection runat="server" topbuttons="true" bottomspacing="5" showsectionline="false" showstandardcancelbutton="false">
		    <Template_Buttons>
			    <asp:Button UseSubmitBehavior="false" runat="server" class="ms-ButtonHeightWidth" OnClick="btnSave_OnClick" 
                    Text="<%$Resources:wss,multipages_okbutton_text%>" id="btnSaveTop" accesskey="<%$Resources:wss,okbutton_accesskey%>" />
			    <asp:Button UseSubmitBehavior="false" runat="server" class="ms-ButtonHeightWidth" PostBackUrl="/" 
                    Text="<%$Resources:wss,multipages_cancelbutton_text%>" id="btnCancelTop" accesskey="<%$Resources:wss,cancelbutton_accesskey%>" 
                    CausesValidation="false"/>
		    </Template_Buttons>
	    </wssuc:buttonsection>
        <colgroup>
            <col style="width: 40%" />
            <col style="width: 60%" />
        </colgroup>
        <tr>
            <td>
                <wssuc:InputFormSection ID="InputFormSection1" runat="server"
                    Title="Picture Site Collection URL"
                    Description="Enter the Site Collection URL to store pictures. This Site Collection should allow Everyone read access to a Picture Library named 'UserPhotos' and the Farm Administrator with Contribute rights.">
                        <template_inputformcontrols>
                            <wssuc:InputFormControl runat="server" LabelText="Site Collection:" ExampleText="https://pictures.example.com/sites/users" 
                                LabelAssociatedControlId="tBox1">
                                <Template_Control>
                                    <div class="ms-authoringcontrols">
                                        <SharePoint:InputFormTextBox runat="server" ID="tBox1" MaxLength="1024" TextMode="SingleLine" Width="60%"/>
                                        <SharePoint:InputFormRequiredFieldValidator runat="server" ID="v1" ErrorMessage="Site Collection Required." 
									        SetFocusOnError="true" ControlToValidate="tBox1" />  
                                        <SharePoint:InputFormRequiredFieldValidator runat="server" ID="v2" ErrorMessage="Please enter a valid URL."
                                            SetFocusOnError="true" ControlToValidate="tBox1" />    
                                    </div>
                                </Template_Control>
                            </wssuc:InputFormControl>
                        </template_inputformcontrols>
                </wssuc:InputFormSection>
            </td>
            <td>
                <wssuc:InputFormSection ID="InputFormSection2" runat="server"
                    Title="Exchange 2013 Web Services URL"
                    Description="If using Exchange 2013, enter the Exchange Web Services (EWS) URL. Use the Internal Outlook Web Access URL in conjunction with '/ews/Exchange.asmx'">
                        <template_inputformcontrols>
                            <wssuc:InputFormControl runat="server" LabelText="Exchange EWS:" ExampleText="https://outlook.example.com/ews/Exchange.asmx" 
                                LabelAssociatedControlId="tBox2">
                                <Template_Control>
                                    <div class="ms-authoringcontrols">
                                        <SharePoint:InputFormTextBox runat="server" ID="tBox2" MaxLength="1024" TextMode="SingleLine" Width="60%"/> 
                                        <SharePoint:InputFormRequiredFieldValidator runat="server" ID="v3" ErrorMessage="Please enter a valid URL."
                                            SetFocusOnError="true" ControlToValidate="tBox2" />  
                                    </div>
                                </Template_Control>
                            </wssuc:InputFormControl>
                        </template_inputformcontrols>
                </wssuc:InputFormSection>
            </td>
        </tr>
        <wssuc:buttonsection runat="server" topbuttons="true" bottomspacing="5" showsectionline="false" showstandardcancelbutton="false">
		    <Template_Buttons>
			    <asp:Button UseSubmitBehavior="false" runat="server" class="ms-ButtonHeightWidth" OnClick="btnSave_OnClick" 
                    Text="<%$Resources:wss,multipages_okbutton_text%>" id="btnSaveBottom" accesskey="<%$Resources:wss,okbutton_accesskey%>" />
			    <asp:Button UseSubmitBehavior="false" runat="server" class="ms-ButtonHeightWidth" PostBackUrl="/" 
                    Text="<%$Resources:wss,multipages_cancelbutton_text%>" id="btnCancelBottom" accesskey="<%$Resources:wss,cancelbutton_accesskey%>" 
                    CausesValidation="false"/>
		    </Template_Buttons>
	    </wssuc:buttonsection>
    </table>
</asp:Content>

<asp:Content ID="PageTitle" ContentPlaceHolderID="PlaceHolderPageTitle" runat="server">
    Configure Foundation Sync
</asp:Content>

<asp:Content ID="PageTitleInTitleArea" ContentPlaceHolderID="PlaceHolderPageTitleInTitleArea" runat="server">
    Configure Foundation Sync
</asp:Content>
