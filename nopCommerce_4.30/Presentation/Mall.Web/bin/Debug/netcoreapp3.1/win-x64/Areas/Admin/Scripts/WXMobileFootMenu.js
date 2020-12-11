function callBackDrpMenu() {
    var linkType = $(this).data("val");    
    if (linkType == "")
    {
        $("#txtshowContent").removeAttr("disabled");
        $("#txtshowContent").val("");
        $("#txtContent").val("");
    }
    else
    {
        $("#txtshowContent").val(linkType);
        $("#txtContent").val(linkType);
    }
    //HiShop.popbox.dplPickerColletion({
    //    linkType: $(this).data("val"),        
    //    callback: function (item, type) {
    //        $("#txtshowContent").val(item.linkType);
    //        $("#txtContent").val(item.linkType);
    //    }
    //});
}

$(function () {
    CreateDropdown($("#txtContent"), $("#uploaderpic"), { createType: 3, showTitle: true, style: "margin-left: 30px;", txtContinuity: false, reWriteSpan: false, callback: callBackDrpMenu }, "vshop");
    loadmenu();
    //initImageUpload();
})
var setmenuid = "";
function onemenu(menuid) {
    setmenuid = menuid;
    bishi = "1";
}
function edittitle(id) {
    $("#txtedittitle" + id).focus();
    $("#spantitlename" + id).css("display", "none");
    $("#spanedittile" + id).css("display", "");
    $("#btnedit" + id).css("display", "none");
    $("#submitedit" + id).css("display", "");
}
function conseleditwin(id) {
    $("#spantitlename" + id).css("display", "");
    $("#spanedittile" + id).css("display", "none");
    $("#btnedit" + id).css("display", "");
    $("#submitedit" + id).css("display", "none");
}
//菜单显示
function showmenu(data) {
    $("#showmenuul").html("");
    $("#showtextul").html("");
    var html = "";
    //if (menudata.shopmenupic == "1") {
    $("#textmenu").remove();

    for (var i = 0; i < data.data.length; i++) {
        var menudata = data.data[i];
        html += "<li class=\"child\">";
        html += "<div class=\"img\">";
        html += " <img src=\"" + menudata.shopmenupic + "\"/>";
        html += "</div>";
        html += "<p>" + menudata.name + "</p>";

        if (menudata.childdata != undefined) {
            if (menudata.childdata.length > 0) {
                html += " <div class=\"inner-nav\"><ul>";
                for (var j in menudata.childdata) {
                    var childmenudata = menudata.childdata[j];
                    html += " <li>" + childmenudata.name + "</li>";
                }
                html += " </ul></div>";
            }
        }
        html += "</li>";
    }
    $("#showmenuul").append(html);
    $('.mobile-nav ul li').not('.mobile-nav ul li li').css('width', $('.mobile-nav').width() / $('.mobile-nav ul li').not('.mobile-nav ul li li').length).hover(function () {
        $(this).find('.inner-nav').show().css({
            'top': -$(this).find('.inner-nav').height() - 20,
            'left': '50%',
            'marginLeft': -$(this).find('.inner-nav').width() / 2
        });
    }, function () {
        $(this).find('.inner-nav').hide();
    })
}
function EnterPress(e, id) {
    var e = e || window.event;
    if (e.keyCode == 13) {
        updatename(id);
    }
}
// 加载菜单列表
function loadmenu() {
    $.post('GetFootMenus', {}, function (d) {
        if (d.status == "0") {
            showmenu(d);
            var menuhtml = "";
            $('#ulmenu li').remove();
            $("#content").html("");
            $('#tabpane').remove();
            var b = 0;
            var menuid = 0;
            if (d.data.length == 5) {
                $("#addmenu").css("display", "none");
            }
            else {
                $("#addmenu").css("display", "");
            }
            for (var i = 0; i < d.data.length; i++) {
                var menudata = d.data[i];
                var active = "";
                if (setmenuid == "menu_" + menudata.menuid)
                    active = "class=\"active\"";
                if (i == 0) {
                    if (i == d.data.length - 1) {
                        active = "class=\"active\"";
                        b = 1;
                    }
                    if (setmenuid == "" && bishi != "0") {
                        active = "class=\"active\"";
                        b = 1;
                    }
                }
                else {
                    b = 0;

                    if (bishi == "0" && (d.data.length - 1) == i) {
                        menuid = menudata.menuid;
                        $("#" + setmenuid.split('_')[1]).removeClass('active');

                    }
                    else
                        menuhtml = "  <div  style=\"display:none\" " + active + " id=\"" + menudata.menuid + "\"><a id=\"menu_" + menudata.menuid + "\"  onclick=\"onemenu('menu_" + menudata.menuid + "')\">" + menudata.name + "<i class=\"del_ic\"   onclick=\"delmenu('" + menudata.menuid + "','0')\"/>×</i></a></div>";
                }
                var childmenuhtml = "<div class=\"active closemenu\" id=\"" + menudata.menuid + "\"><a id=\"menu_" + menudata.menuid + "\"  onclick=\"onemenu('menu_" + menudata.menuid + "')\"><i class=\"del_ic\"   onclick=\"delmenu('" + menudata.menuid + "','0')\">×</i></a></div>";
                var js = 0;

                $("#addmenu").before(menuhtml);//添加父菜单的Tab选项卡
                var tabcontent = $($("#tabcontent").html());
                tabcontent.find('#fltitle').text(menudata.name);
                tabcontent.find('#EditMenu').attr("onclick", "addandeditmenu('1','" + menudata.menuid + "','','one')");

                tabcontent.find('#addtwomenu').attr("onclick", "addandeditmenu('0','','" + menudata.menuid + "','two')");
                tabcontent.find("#childmenu").append(childmenuhtml);//添加子菜单

                $("#content").append(tabcontent);//添加父菜单
                tabcontent.find("#tabpane").attr("id", "tabmenu_" + menudata.menuid + "");
                tabcontent.find("#tabmenu_" + menudata.menuid).parent('#panediv').attr("id", "toptabmenu_" + menudata.menuid);
                if (setmenuid == "" && b == 1) {

                    $("#toptabmenu_" + menudata.menuid).addClass('active');
                }
                else {

                    if (bishi == "0") {
                        $("#toptabmenu_" + setmenuid.split('_')[1]).removeClass('active');
                        $("#toptabmenu_" + menuid).addClass('active');
                    }
                    else {
                        $("#toptabmenu_" + setmenuid.split('_')[1]).addClass('active');
                    }
                }
                if (menudata.childdata != undefined) {
                    if (menudata.childdata.length == 0) {
                        tabcontent.find("#spanhid").text("");
                    }
                }
            }
            setload();
        }
    });
}
function updatename(id) {
    var name = escape($("#txtedittitle" + id).val());
    var url = "&MenuId=" + id + "&Name=" + name;
    if ($.trim(name) == "") {
        $.dialog.tips('请填写标题');
        return;
    }
    if ($.trim(name).length > 7) {
        $.dialog.tips('二级菜单标题不能大于7个字！');
        return;
    }
    $.ajax({
        type: "POST",
        url: "../../API/MenuProcess.ashx?action=updatename" + url,
        success: function (d) {
            if (d.status == "0") {
                loadmenu();
                conseleditwin(id);
                $("#spantitlename" + id).text(name);
                $.dialog.tips('修改成功');
            }
            else {
                $.dialog.tips('修改失败');
            }
        }
    });
}
//加载Tab选项卡
function setload() {
    $('#mytabl > ul li').click(function () {
        $('#mytabl > ul li').removeClass('active');
        $(this).addClass('active');
        $(this).parent().next().children().removeClass('active');
        $(this).parent().next().children().eq($(this).index()).addClass('active');
    })
}
var andedit;
var bishi;
var editid;
var parentid;
//打开窗口
function addandeditmenu(type, id, parentmenuid, oneortwo) {
    andedit = type;
    if (oneortwo == "two") {
        //initValid(new InputValidator('txttitle', 1, 4, false, null, '不能为空，且必须在4个字符以内'));
    }
    if (parentmenuid == "") {
        bishi = "0";
    }

    if (oneortwo == "one") {
        //initValid(new InputValidator('txttitle', 1, 4, false, null, '不能为空，且必须在4个字符以内'));
    }
    if (type == "0") {
        bishi = "0";
    }
    else {
        if (parentmenuid == "" && bishi != "0")
            bishi = "1";
    }
    if (parentmenuid == "") {
        $("#uploaderpic").css("display", "");
    } else {

        $("#uploader1_image").remove();
        $("#uploaderpic").css("display", "none");
        $("#uploader1_upload").css("display", "");
        $("#uploader1_delete").css("display", "none");
    }
    if (oneortwo == "one") {
        $("#linkem").css('display', 'none')
    }
    else {
        $("#linkem").css('display', '')
    }
    editid = id;
    parentid = parentmenuid
    $("#txttitle").val('');
    $("#txtContent").val('');
    $("#txtshowContent").val('');
    if (type == 0) {
        //$("#<%=hidOldLogo.ClientID%>").val("");
        //$("#<%=hidUploadLogo.ClientID%>").val("");
        //initImageUpload();
        $('#MenuIconBox').val('');
        $("#modaltitle").text('添加导航');

        $("#menuiconUrl").MallUpload({
            title: '图标：',
            imageDescript: '请上传60*60的图片',
            displayImgSrc: $('#MenuIconBox').val(),
            imgFieldName: "menuiconUrl",
            dataWidth: 7
        });
    }
    else {
        $("#modaltitle").text('修改导航');
        $.post('GetFootMenuInfoById', { id: id }, function (d) {
            if (d.status == "0") {
                var data = d.data;
                $("#txttitle").val(data.name);
                $("#txtContent").val(data.content);
                $("#txtshowContent").val(data.content);
                $('#MenuIconBox').val(data.shopmenupic);

                $("#menuiconUrl").MallUpload({
                    title: '图标：',
                    imageDescript: '请上传60*60的图片',
                    displayImgSrc: $('#MenuIconBox').val(),
                    imgFieldName: "menuiconUrl",
                    dataWidth: 7
                });
            }
            else {
                $.dialog.tips('查询导航失败');
            }
        });
    }

   

    $('#myModal').modal('toggle').children().css({
        width: '600px',
        height: '500px'
    })
    $("#myModal").modal({ show: true });
}
//添加和修改菜单
function submitaddandedit() {
    //getUploadImages();
    if ($.trim($("#txttitle").val()) == "") {
        $.dialog.tips('请填写标题！');
        return;
    }
    if (parentid == "") {
        if ($.trim($("#txttitle").val()).length > 4) {
            $.dialog.tips('一级导航标题最多只能添加4个字！');
            return;
        }
    }
    else {
        if ($.trim($("#txttitle").val()).length > 7) {
            $.dialog.tips('二级导航标题不能大于7个字');
            return;
        }
        if ($.trim($("#txtContent").val()) == "") {
            $.dialog.tips('链接内容不能为空');
            return;
        }
    }
    var Type = 'click';
    if (mestype != 0)
        Type = 'view';
    if (andedit == 0) {//添加一级和二级菜单
        var pic = $("#menuiconUrl").MallUpload('getImgSrc');
        var url = $("#txtContent").val();
        if (url == "") {
            url = $("#txtshowContent").val();
        }
        $.post('AddFootMenu', { id: editid, description: escape($("#txttitle").val()), imageUrl: pic, url: url }, function (d) {
            if (d.status == "0") {
                $.dialog.tips('添加成功');
                loadmenu();
                $('#myModal').modal('hide')
            }
            else {
                if (d.status == "1") {
                    $.dialog.tips('添加导航失败');
                }
                else {
                    $.dialog.tips('导航最多只能添加5个');
                }
            }
        });
    }
    if (andedit == 1)//修改一级和二级菜单
    {
        var pic = $("#menuiconUrl").MallUpload('getImgSrc');
        var url = $("#txtContent").val();
        if (url == "")
        {
            url = $("#txtshowContent").val();
        }
        $.post('AddFootMenu', { id: editid, description: escape($("#txttitle").val()), imageUrl: pic, url: url }, function (d) {
            if (d.status == "0") {
                $.dialog.tips('修改导航成功');
                loadmenu();
                $('#myModal').modal('hide')
            }
            else {
                $.dialog.tips('修改导航失败');
            }
        });
    }
}
function delmenu(id, type) {
    if (confirm("确定要删除数据吗？")) {
        var mid= id;
        $.post('DelFootMenu', { id: mid }, function (d) {
            if (d.status == "0") {
                if (type == 0) {
                    setmenuid = "";
                }
                $('#ulmenu li').remove();
                $("#content").html("");
                $('#tabpane').remove();
                loadmenu();
                $.dialog.tips('删除成功');
            }
            else {
                $.dialog.tips('删除失败');
            }
        });
    }
}
function jsopenemotion() {
    var EmotionFace = weiboHelper.options.Emotions;
    if ($(".emotion-wrapper").is(":visible")) {
        $(".emotion-wrapper").hide()
    } else {
        var emotionHtml = "";
        for (var i = 0; i < EmotionFace.length; i++) {
            emotionHtml += '<li><img src="http://img.t.sinajs.cn/t4/appstyle/expression/ext/normal/' + EmotionFace[i][0] + '" alt="[' + EmotionFace[i][1] + ']" title="[' + EmotionFace[i][1] + ']"></li>';
        }
        $(".emotion-container").html(emotionHtml);
        $(".emotion-wrapper").show("slow", function () {
            $(".emotion-container img").click(function () {
                $("#txtMessageContent").val($("#txtMessageContent").val() + ($(this).attr("alt"))).keyup();
                $(".emotion-wrapper").hide()
            })
        });
    }
}
function contentchange() {
    $("#txtshowContent").val($("#txtContent").val());
}
var mestype = 0;
function messagetype(type) {
    mestype = type;
}
function okhttp() {
    var content = "http://";
    if ($.trim($("#txthttp").val()) == "") {
        $.dialog.tips('请输入链接地址！');
        return;
    }
    $("#txtContent").val('');
    $("#txtshowContent").val('');
    $("#txtContent").val($("#txthttp").val());
    $("#txtshowContent").val($("#txthttp").val());
    if ($("#txthttp").val().indexOf('http://') == -1) {
        $("#txtContent").val(content + $("#txthttp").val());
        $("#txtshowContent").val(content + $("#txthttp").val());
    }
    $("#myOutHttpModal").modal('hide');
}
function showhttp(type) {
    $("#txthttp").val('');
    mestype = type;
    $('#myOutHttpModal').modal('toggle').children().css({
        width: '500px',
        height: '100px'
    })
    $("#myOutHttpModal").modal({ show: true });

}

function setEnable(obj) {
    var ob = $("#" + obj.id);
    var cls = ob.attr("class");
    var enable = "false";
    if (cls == "switch-btn") {

        ob.empty();
        ob.append("已关闭 <i></i>")
        ob.removeClass();
        ob.addClass("switch-btn off");
        enable = "false";

    }
    else {
        ob.empty();
        ob.append("已开启 <i>OFF</i>")
        ob.removeClass();
        ob.addClass("switch-btn");
        enable = "true";
    }
    $.ajax({
        type: "post",
        url: "../../API/MenuProcess.ashx",
        data: { type: "1", enable: enable, action: "setenable" },
        dataType: "text",
        success: function (data) {
            if (enable == 'true') {

                msg('已开启！');

            }
            else {
                msg('已关闭！');

            }
            loadmenu();
        }
    });
}
function msg(msg) {
    $.dialog.tips(msg);
}

function errAlert(msg) {
    $.dialog.tips(msg);
}
$(function () {
    $('body').on('mouseover', '#mytabl ul li', function () {
        $(this).find('i').show();
    });
    $('body').on('mouseout', '#mytabl ul li', function () {
        $(this).find('i').hide();
    });
})



// 初始化图片上传控件
function initImageUpload() {
    var logoSrc = $('#<%=hidOldLogo.ClientID%>').val();
    $('#icoContainer span[name="siteLogo"]').MallUpload(
                   {
                       title: '商城logo',
                       url: "/admin/UploadHandler.ashx?action=newupload",
                       imageDescript: '',
                       displayImgSrc: logoSrc,
                       imgFieldName: "siteLogo",
                       defaultImg: '',
                       pictureSize: '60*60',
                       imagesCount: 1,
                       dataWidth: 9
                   });
}

//获取上传成功后的图片路径
function getUploadImages() {
    var srcLogo = $('#icoContainer span[name="siteLogo"]').MallUpload("getImgSrc");
    $("#<%=hidUploadLogo.ClientID%>").val(srcLogo);
}

