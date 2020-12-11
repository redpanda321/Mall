$(function () {
    query();
    $("#searchBtn").click(function () { query(); });
    AutoComplete();
    $("#btncoupon").click(function () {
        bind();
    });
    $('#msgtype_news').click(function () {
        $('#mediaSelect').show();
        $('#txtInput').hide();
        $('#msgtype_text').removeClass('active');
        $(this).addClass('active');
    });
    $('#msgtype_text').click(function () {
        $('#txtInput').show();
        $('#mediaSelect').hide();
        $('#msgtype_news').removeClass('active');
        $(this).addClass('active');
    });


    $(".tab-content .library").click(function () {
        $(".sucai-library").show();
        $(".coverage").show();
        GetMaterialData();
    });

    $(".sucai-library .glyphicon-remove").click(function () {
        $(".sucai-library").hide();
        $(".coverage").hide();
    });
    $('#btnCancel').click(function () {
        $(".sucai-library").hide();
        $(".coverage").hide();
    });

    $('#btnOk').click(function () {
        var $li = $('li[curMedia=1]');
        if ($li.length > 0) {
            curMedia = $li.attr('id');
            $(".sucai-library").hide();
            $(".coverage").hide();
            $(".create_access").hide();

            var curMediaObj = mediaList[$li.attr('idx')];
            var $detail = $('#mediaDetail');
            $('#mediaTime').val(curMediaObj.update_time);
            $('img[name=wrapper]').attr('src', 'GetMedia?mediaid=' + curMediaObj.items[0].thumb_media_id);
            $('span[name=wrapperTitle]').text(curMediaObj.items[0].title);

            var html = [];
            $(curMediaObj.items).each(function (idx, el) {
                if (idx > 0) {
                    html.push(' <div class="item" >');
                    html.push(' <div class="WX-edted">');
                    html.push(' <i><img src=GetMedia?mediaid=' + $(el).attr('thumb_media_id') + ' /> </i>');
                    html.push(' <span name="title">' + $(el).attr('title') + '</span>');
                    html.push('</div></div>');
                }
            });
            $('#divChild').html(html.join(''));
            $detail.show();
        }
    });

    $("#inputStartDateLogin,#inputStartDate").datetimepicker({
        language: 'zh-CN',
        format: 'yyyy-mm-dd',
        autoclose: true,
        weekStart: 1,
        minView: 2
    });
    $("#inputEndDateLogin,#inputEndDate").datetimepicker({
        language: 'zh-CN',
        format: 'yyyy-mm-dd',
        autoclose: true,
        weekStart: 1,
        minView: 2
    });

    $("#inputStartDate").on('changeDate', function () {
        if ($("#inputEndDate").val()) {
            if ($("#inputStartDate").val() > $("#inputEndDate").val()) {
                $("#inputEndDate").val($("#inputStartDate").val());
            }
        }
        $("#inputEndDate").datetimepicker('setStartDate', $("#inputStartDate").val());
    });
    $("#inputStartDateLogin").on('changeDate', function () {
        if ($("#inputEndDateLogin").val()) {
            if ($("#inputStartDateLogin").val() > $("#inputEndDateLogin").val()) {
                $("#inputEndDateLogin").val($("#inputStartDateLogin").val());
            }
        }

        $("#inputEndDateLogin").datetimepicker('setStartDate', $("#inputStartDateLogin").val());
    });
    $('#btnAdvanceSearch').click(function () {
        $('#divAdvanceSearch').toggle();

        if ($(this).hasClass('menu-shrink')) {
            $(this).removeClass("menu-shrink").addClass("up-shrink")
        } else {
            $(this).addClass("menu-shrink").removeClass("up-shrink")
        }
    });
});
function Delete(id) {
    $.dialog.confirm('确定删除该用户吗？', function () {
        var loading = showLoading();
        $.post("./Delete", { id: id }, function (data) { $.dialog.tips(data.msg); query(); loading.close(); });
    });
}
function Lock(id) {
    $.dialog.confirm('冻结之后，会员将不能登录商城，您确定冻结？', function () {
        var loading = showLoading();
        $.post("./Lock", { id: id }, function (data) { $.dialog.tips(data.msg); query(); loading.close(); });
    });
}
function UnLock(id) {
    $.dialog.confirm('确定重新激活该用户吗？', function () {
        var loading = showLoading();
        $.post("./UnLock", { id: id }, function (data) { $.dialog.tips(data.msg); query(); loading.close(); });
    });
}

function BatchLock() {
    var selectedRows = $("#list").MallDatagrid("getSelections");


    if (selectedRows.length == 0) {
        $.dialog.tips("你没有选择任何选项！");
    }
    else {
        var selectids = new Array();
        for (var i = 0; i < selectedRows.length; i++) {
            selectids.push(selectedRows[i].Id);
        }
        $.dialog.confirm('确定冻结选择的用户吗？', function () {
            var loading = showLoading();
            $.post("./BatchLock", { ids: selectids.join(',') }, function (data) { $.dialog.tips(data.msg); query(); loading.close(); });
        });
    }
}
$(function () {
    $("#divSetLabel .form-group").css({ "width": "150px", "float": "left", "border": "none", "white-space": "nowrap", "overflow": "hidden", "margin": "10px" });
});

function Show(id) {
    var str = '';
    var loading = showLoading();
    $.ajax({
        type: "post",
        async: true,
        dataType: "html",
        url: '/Admin/member/Detail',
        data: { Id: id },
        success: function (data) {
            str = data;
            $.dialog({
                title: '会员信息',
                lock: true,
                id: 'ChangePwd',
                width: '400px',
                content: str,
                padding: '0 40px',
                okVal: '确定',
                ok: function () {
                }
            });
            loading.close();
        }
    });
}
function BatchDelete() {
    var selectedRows = $("#list").MallDatagrid("getSelections");
    var selectids = new Array();

    for (var i = 0; i < selectedRows.length; i++) {
        selectids.push(selectedRows[i].Id);
    }
    if (selectedRows.length == 0) {
        $.dialog.tips("你没有选择任何选项！");
    }
    else {
        $.dialog.confirm('确定删除选择的用户吗？', function () {
            var loading = showLoading();
            $.post("./BatchDelete", { ids: selectids.join(',') }, function (data) { $.dialog.tips(data.msg); query(); loading.close(); });
        });
    }
}

function query() {
    $("#list").MallDatagrid({
        url: './list',
        nowrap: false,
        rownumbers: true,
        NoDataMsg: '没有找到符合条件的数据',
        border: false,
        fit: true,
        fitColumns: true,
        pagination: true,
        idField: "Id",
        pageSize: 10,
        pageNumber: 1,
        queryParams: getQueryParams(),
        toolbar: /*"#goods-datagrid-toolbar",*/'',
        operationButtons: "#batchOperate",
        columns:
        [[
            { checkbox: true, width: 39 },
            { field: "Id", hidden: true },
            {
                field: "UserName", title: '会员名',
                formatter: function (value, row, index) {
                    var html = '<img src="' + row.IconSrc + '" title="' + row.PlatformText + '" width="16" style="margin:5px 0 0 5px;">' + value;
                    return html;
                }
            },
            { field: "Nick", title: '昵称' },
            { field: "GradeName", title: '等级', width: 50 },
            { field: "AvailableIntegral", title: '积分', width: 60, sort:true },
            { field: "NetAmount", title: '净消费', sort: true },
            { field: "CellPhone", title: '手机' },
            { field: "CreateDateStr", title: '创建日期', sort: true},
            {
                field: "Disabled", title: '状态',
                formatter: function (value, row, index) {
                    var html = "";
                    if (row.Disabled)
                        html += '冻结';
                    else
                        html += '正常';
                    return html;
                }
            },
        {
            field: "operation", operation: true, title: "操作",
            formatter: function (value, row, index) {
                var id = row.Id.toString();
                var html = ["<span class=\"btn-a\">"];
                html.push("<a onclick=\"AddLabel('" + id + "');\">编辑标签</a>");
                html.push("<a href='MemberDetail/" + id + "'>查看</a>");
                html.push("<a onclick=\"ChangePassWord('" + id + "');\">修改密码</a>");
                if (row.Disabled)
                    html.push("<a onclick=\"UnLock('" + id + "');\">解冻</a>");
                else
                    html.push("<a onclick=\"Lock('" + id + "');\">冻结</a>");
                //html.push("<a onclick=\"Delete('" + id + "');\">删除</a>");

                html.push("</span>");
                return html.join("");
            }
        }
        ]]
    });
}

function AddLabel(memberid) {
    if ($('input[name=check_Label]').length == 0) {
        $.dialog.alert('没有可添加的标签，请到标签管理添加！');
        return;
    }
    $.ajax({
        type: 'post',
        url: 'GetMemberLabel',
        data: { id: memberid },
        success: function (data) {
            $('input[name=check_Label]').each(function (i, checkbox) {
                $(checkbox).get(0).checked = false;
            });
            $.each(data.Data, function (i, item) {
                $('#check_' + item.LabelId).get(0).checked = true;
            });
            $.dialog({
                title: '会员标签',
                lock: true,
                id: 'SetLabel',
                width: '630px',
                content: document.getElementById("divSetLabel"),
                padding: '10px 60px',
                okVal: '确定',
                ok: function () {
                    var ids = [];
                    $('input[name=check_Label]').each(function (i, checkbox) {
                        if ($(checkbox).get(0).checked) {
                            ids.push($(checkbox).attr('datavalue'));
                        }
                    });
                    var loading = showLoading();
                    $.post('SetMemberLabel', { id: memberid, labelids: ids.join(',') }, function (result) {
                        if (result.Success) {
                            query();
                            $.dialog.tips('设置成功！');
                        }
                        loading.close();
                    });
                }
            });
        }
    });

}

function batchAddLabels() {
    var selecteds = $("#list").MallDatagrid('getSelections');
    var ids = [];
    $.each(selecteds, function () {
        ids.push(this.Id);
    });
    if (ids.length == 0) {
        $.dialog.tips('请选择会员！');
        return;
    }
    $('input[name=check_Label]').each(function (i, checkbox) {
        $(checkbox).get(0).checked = false;
    });

    $.dialog({
        title: '会员标签',
        lock: true,
        id: 'SetLabel',
        width: '630px',
        content: document.getElementById("divSetLabel"),
        padding: '0 40px',
        okVal: '确定',
        ok: function () {
            var labelids = [];
            $('input[name=check_Label]').each(function (i, checkbox) {
                if ($(checkbox).get(0).checked) {
                    labelids.push($(checkbox).attr('datavalue'));
                }
            });
            var loading = showLoading();
            $.post('SetMembersLabel', { ids: ids.join(','), labelids: labelids.join(',') }, function (result) {
                if (result.Success) {
                    query();
                    $.dialog.tips('设置成功！');
                }
                loading.close();
            });
        }
    });
}
function ChangePassWord(id) {
    var pwdreg = /^[^\s]{6,20}$/;

    $.dialog({
        title: '修改密码',
        lock: true,
        id: 'ChangePwd',
        content: document.getElementById("dialogform"),
        padding: '0 40px',
        okVal: '确定',
        init: function () { $("#password").focus(); },
        ok: function () {
            var passwpord = $("#password").val();
            if (!pwdreg.test(passwpord)) {
                $.dialog.errorTips("密码长度至少是6-20位,且不能包含空格！");
                return false;
            }
            var loading = showLoading();
            $.post("./ChangePassWord", { id: id, password: passwpord }, function (data) { $.dialog.tips(data.msg); $("#password").val(""); loading.close(); });
        }
    });

}

function AutoComplete() {
    //autocomplete
    $('#autoTextBox').autocomplete({
        source: function (query, process) {
            var matchCount = this.options.items;//返回结果集最大数量
            $.post("./getMembers", { "keyWords": $('#autoTextBox').val() }, function (respData) {
                return process(respData);
            });
        },
        formatItem: function (item) {
            return item.value;
        },
        setValue: function (item) {
            return { 'data-value': item.value, 'real-value': item.key };
        }
    });
}

function getQueryParams() {
    var rtstart, rtend, ltstart, ltend, isseller, isfocus, labelid, platform;
    if ($('#divAdvanceSearch').css('display') != 'none') {
        rtstart = $("#inputStartDate").val();
        rtend = $("#inputEndDate").val();
        ltstart = $("#inputStartDateLogin").val();
        ltend = $("#inputEndDateLogin").val();
        isseller = $("#isrzseller").val();
        isfocus = $("#iswxfocus").val();
    }
    if ($('#labelSelect').val() > 0) {
        labelid = $('#labelSelect').val();
    }

    var gradeId = $("#grade").val();
    var mobile = $("#mobile").val();
    var status = $("#status").val();
    platform = $("#platform").val();
    var weChatNick = $("#weChatNick").val();
    var result = {
        keyWords: $("#autoTextBox").val()
    };
    if(labelid && labelid.length>0){
        result.labels = [labelid];
    }
    if (isseller && isseller.length > 0) {
        result.isSeller = isseller;
    }
    if (isfocus && isfocus.length > 0) {
        result.isFocusWeiXin = isfocus;
    }
    if(rtstart && rtstart.length>0){
        result.registTimeStart=rtstart;
    }
    if(rtend && rtend.length>0){
        result.registTimeEnd=rtend;
    }
    if(gradeId && gradeId.length>0){
        result.gradeId=gradeId;
    }
    if(mobile && mobile.length>0){
        result.mobile=mobile;
    }
    if(weChatNick && weChatNick.length>0){
        result.weChatNick=weChatNick;
    }
    if(status && status.length>0){
        result.status=status;
    }
    if (platform) {
        result.platform = platform;
    }
    return result;
}

function ExportExecl() {
    var url = "/Admin/Member/ExportToExcel?";
    var urldata = getQueryParams();
    var ltstart, ltend;
    if ($('#divAdvanceSearch').css('display') != 'none') {
        ltstart = $("#inputStartDateLogin").val();
        ltend = $("#inputEndDateLogin").val();
        if (ltstart && ltstart.length > 0) {
            urldata.logintimeStart = ltstart;
        }
        if (ltstart && ltstart.length > 0) {
            urldata.logintimeEnd = ltend;
        }
    }

    for (var item in urldata) {
        var itemdata = urldata[item];
        if (itemdata) {
            url += item + "=" + itemdata + "&";
        }
    }
    $("#aExport").attr("href", url);
}


function sendSms(type) {
    switch (type) {
        case "check": ChoceSms(); break;
        case "result": ChoceSmsAll(); break;
    }
}

function ChoceSms() {
    var selecteds = $("#list").MallDatagrid('getSelections');
    var ids = [];
    $.each(selecteds, function (i,v) {
        ids.push(v.Id);
    });
    if (ids.length == 0) {
        $.dialog.tips('请选择会员！');
        return;
    }
    //debugger
    $('input[name=check_Label]').each(function (i, checkbox) {
        $(checkbox).get(0).checked = false;
    });

    $("#contentDesc1").val("");
    $.dialog({
        title: '发送短信',
        lock: true,
        id: 'SendSms',
        width: '630px',
        content: document.getElementById("divSendSms"),
        padding: '0 40px',
        okVal: '确定',
        ok: function () {

            if ($("#contentDesc1").val() == "") {
                $.dialog.tips('请输入发送短信内容！');
                return false;
            }

            var loading = showLoading();
            $.post('SendSms', { ids: ids.join(','), sendCon: $("#contentDesc1").val() }, function (result) {
                if (result.success) {
                    query();
                    $.dialog.tips('操作成功！等待短信平台发送消息。');
                } else {
                    $.dialog.errorTips('发送失败！');
                }
                loading.close();
            });
        }
    });
}

function ChoceSmsAll() {
    var selecteds = $("#list").MallDatagrid('getSelections');
    //debugger
    $('input[name=check_Label]').each(function (i, checkbox) {
        $(checkbox).get(0).checked = false;
    });
    $("#contentDesc1").val("");
    $.dialog({
        title: '发送短信',
        lock: true,
        id: 'SendSms',
        width: '630px',
        content: document.getElementById("divSendSms"),
        padding: '0 40px',
        okVal: '确定',
        ok: function () {
            if ($("#contentDesc1").val() == "") {
                $.dialog.tips('请输入发送短信内容！');
                return false;
            }
            var para = getQueryParams();
            var loading = showLoading();
            $.post('SendAllSmsBymember', { sendCon: $("#contentDesc1").val(), keyWords: para.keyWords, Status: para.status, PageNo: para.page, PageSize: para.rows, IsSeller: para.isSeller, IsFocusWeiXin: para.isFocus, Mobile: para.mobile, GradeId: para.gradeId, weChatNick: para.weChatNick, RegistTimeStart: para.regtimeStart, RegistTimeEnd: para.regtimeEnd, LabelId: para.labelid, Platform: para.platform }, function (result) {
                if (result.success) {
                    query();
                    $.dialog.tips('操作成功！等待短信平台发送消息。');
                } else {
                    $.dialog.tips(result.msg);
                }
                loading.close();
            });
        }
    });
}


function sendCoupon(type) {
    switch (type) {
        case "check": ChoceCoupon(); break;
        case "result": ChoceCouponAll(); break;
    }
}

function ChoceCoupon() {
    var selecteds = $("#list").MallDatagrid('getSelections');

    var ids = [];
    $.each(selecteds, function (i, v) {
        ids.push(v.Id);
    });

    if (ids.length == 0) {
        $.dialog.tips('请选择会员！');
        return;
    }
    $("#couponName").val("");
    $.dialog({
        title: '选取优惠券',
        lock: true,
        width: 650,
        padding: '0 5px',
        id: 'chooseCouponDialog',
        content: $('#choceCouponUrl')[0],
        okVal: '确认赠送',
        ok: function () {
            if ($('#colist tr').length + $('input[name="topic"]:checked').length > 99) {
                $.dialog.tips('发送的优惠券总数不能超过99！');
                return false;
            }

            var selecteds = $("#list").MallDatagrid('getSelections');
            var ids = [];
            $.each(selecteds, function (i, v) {
                ids.push(v.Id);
            });

            selecteds = $("#CouponGrid").MallDatagrid('getSelections');
            var couponIds = [];
            $.each(selecteds, function () {
                couponIds.push(this.id);
            });
            if (couponIds.length == 0) {
                $.dialog.tips('请选择优惠券！');
                return;
            }


            var loading = showLoading();
            $.post("SendCoupon", { ids: ids.join(','), couponIds: couponIds.join(',') }, function (data) {
                if (data.success) {
                    $.dialog.tips('发送成功！');
                } else {
                    $.dialog.tips(data.msg);
                }
                loading.close();
            }, "json");
        }
    });

    bind();
}

function ChoceCouponAll() {
    var selecteds = $("#list").MallDatagrid('getSelections');

    $("#couponName").val("");
    $.dialog({
        title: '选取优惠券',
        lock: true,
        width: 650,
        padding: '0 5px',
        id: 'chooseCouponDialog',
        content: $('#choceCouponUrl')[0],
        okVal: '确认赠送',
        ok: function () {
            if ($('#colist tr').length + $('input[name="topic"]:checked').length > 99) {
                $.dialog.tips('发送的优惠券总数不能超过99！');
                return false;
            }

            var selecteds = $("#list").MallDatagrid('getSelections');
            var ids = [];
            $.each(selecteds, function () {
                ids.push(this.id);
            });

            selecteds = $("#CouponGrid").MallDatagrid('getSelections');
            var couponIds = [];
            $.each(selecteds, function () {
                couponIds.push(this.id);
            });
            if (couponIds.length == 0) {
                $.dialog.tips('请选择优惠券！');
                return;
            }

            var para = getQueryParams();
            var loading = showLoading();
            $.post('SendAllCouponBymember', { couponIds: couponIds.join(','), keyWords: para.keyWords, Status: para.status, PageNo: para.page, PageSize: para.rows, IsSeller: para.isSeller, IsFocusWeiXin: para.isFocus, Mobile: para.mobile, GradeId: para.gradeId, weChatNick: para.weChatNick, RegistTimeStart: para.regtimeStart, RegistTimeEnd: para.regtimeEnd, LabelId: para.labelid, Platform: para.platform }, function (result) {
                if (result.success) {
                    query();
                    $.dialog.tips('发送成功！');
                } else {
                    $.dialog.tips(result.msg);
                }
                loading.close();
            });
        }
    });

    bind();
}
function bind() {
    //优惠券表格
    $("#CouponGrid").MallDatagrid({
        url: 'CouponList',
        nowrap: false,
        rownumbers: true,
        NoDataMsg: '没有找到符合条件的数据',
        border: false,
        fit: true,
        fitColumns: true,
        pagination: true,
        idField: "id",
        pageSize: 5,
        pageNumber: 1,
        queryParams: { Name: $("#couponName").val(), PageNo: 1, pageSize: 5 },
        columns:
             [[
             { checkbox: true, width: 50 },
            { field: "couponName", title: "优惠券名称", align: "center" },
            { field: "shopName", title: "商家", align: "center" },
            { field: "price", title: "面额", align: "center" },
            {
                field: "inventory", title: "剩余数量", align: "center",
                formatter: function (value, row) {
                    return row.permax == 0 ? '不限张' : value;
                }
            },
            { field: "orderAmount", title: "使用条件", align: "center" },
            {
                field: "strEndTime", title: "有效期", width: 100, align: 'center',
                formatter: function (value, row) {
                    return row.strStartTime + '至' + row.strEndTime;
                }
            },
            
            { field: "id", hidden: true }
             ]]
    });
}

function sendWei(type) {
    switch (type) {
        case "check": ChoceWei(); break;
        case "result": ChoceWeiAll(); break;
    }
}

var pageTotal = 0;
var pageIdx = 1;
var pageSize = 8;
var curMedia = '';
var mediaList = {};

function ChoceWei() {
    var selecteds = $("#list").MallDatagrid('getSelections');
    var ids = [];
    $.each(selecteds, function (i, v) {
        ids.push(v.Id);
    });
    if (ids.length == 0) {
        $.dialog.tips('请选择会员！');
        return;
    }
    //debugger
    $('input[name=check_Label]').each(function (i, checkbox) {
        $(checkbox).get(0).checked = false;
    });

    $.dialog({
        title: '选择图文素材',
        lock: true,
        id: 'SendSms',
        width: '630px',
        content: document.getElementById("divSendWei"),
        padding: '0 40px',
        okVal: '确定',
        ok: function () {

            var msgtype = $('#msgtype').children('.active').attr('value');
            var msgcontent = '';
            if (msgtype == 1) {
                msgcontent = $('#txtInput textarea').val();
                if (msgcontent == '') {
                    $.dialog.alert('发送内容不能为空');
                    return;
                }
                if (msgcontent.length > 600) {
                    $.dialog.alert('发送内容长度不能超过600');
                    return;
                }
            }
            else {
                if (curMedia == 'p') {
                    $.dialog.alert('请选择素材模板');
                    return;
                }
            }
            loading = showLoading('正在发送');
            $.post('SendWXGroupMessage', { ids: ids.join(','), msgtype: msgtype, mediaid: curMedia, msgcontent: msgcontent }, function (result) {
                loading.close();
                if (result.success) {
                    $.dialog.alert('发送成功');
                }
                else {
                    $.dialog.alert(result.msg);
                }
            });
        }
    });
}

function ChoceWeiAll() {
    var selecteds = $("#list").MallDatagrid('getSelections');
    //debugger
    $('input[name=check_Label]').each(function (i, checkbox) {
        $(checkbox).get(0).checked = false;
    });
    $.dialog({
        title: '选择图文素材',
        lock: true,
        id: 'SendSms',
        width: '630px',
        content: document.getElementById("divSendWei"),
        padding: '0 40px',
        okVal: '确定',
        ok: function () {
            var msgtype = $('#msgtype').children('.active').attr('value');
            var msgcontent = '';
            if (msgtype == 1) {
                msgcontent = $('#txtInput textarea').val();
                if (msgcontent == '') {
                    $.dialog.alert('发送内容不能为空');
                    return;
                }
                if (msgcontent.length > 600) {
                    $.dialog.alert('发送内容长度不能超过600');
                    return;
                }
            }
            else {
                if (curMedia == 'p') {
                    $.dialog.alert('请选择素材模板');
                    return;
                }
            }
            var para = getQueryParams();
            var loading = showLoading();
            $.post('SendAllWXGroupMessageByMember', { msgtype: msgtype, mediaid: curMedia, msgcontent: msgcontent, keyWords: para.keyWords, Status: para.status, PageNo: para.page, PageSize: para.rows, IsSeller: para.isSeller, IsFocusWeiXin: para.isFocus, Mobile: para.mobile, GradeId: para.gradeId, weChatNick: para.weChatNick, RegistTimeStart: para.regtimeStart, RegistTimeEnd: para.regtimeEnd, LabelId: para.labelid, Platform: para.platform }, function (result) {
                if (result.success) {
                    query();
                    $.dialog.tips('发送成功！');
                } else {
                    $.dialog.tips(result.msg);
                }
                loading.close();
            });
        }
    });
}

function GetMaterialData() {
    $.post('GetWXMaterialList', { pageIdx: pageIdx, pageSize: pageSize }, function (data) {
        var returnCode = data.errCode || '0';
        if (data.msg) {
            $('#list').append('<li class="con-frame"><div class="source-l">' + data.msg + '</div></li>');
        }
        else {
            if (data.errMsg) {
                $('#list').append('<li class="con-frame">' + data.errMsg + '</li>');
            }
            else {
                var html = [], lihtml = [], mediaid = '';
                $('#list').html('');
                mediaList = data.content;
                $(data.content).each(function (idx, el) {
                    lihtml = [];
                    mediaid = '';
                    $(el.items).each(function (i, item) {
                        if (mediaid == '')
                            mediaid = item.thumb_media_id;
                        lihtml.push('<li>' + item.title + '</li>');
                    });
                    html.push("<li idx=" + idx + " id=\"" + el.media_id + "\" class=\"con-frame\"  onclick=\"selectMaterial('" + el.media_id + "')\">");
                    html.push('<div class="source-l">');
                    html.push('<span><img src="GetMedia?mediaid=' + mediaid + '"></span>');
                    html.push('<ol>');
                    html.push(lihtml.join(''));
                    html.push('</ol>');
                    html.push('</div>');
                    html.push('<div class="source-M"><time>' + el.update_time + '</time></div>');
                    html.push('<span class="SCover"></i></span>');
                    html.push('<i class="glyphicon glyphicon-ok">');
                    html.push('</li>');
                    $('#list').append(html.join(''));
                    html = [];
                });
                $(".con-frame").hover(function () {
                    $(".SCover", this).show();
                }, function () {
                    if ($('.glyphicon-ok', this).css('display') != 'block') {
                        $(".SCover", this).hide();
                    }
                });
                $(".con-frame").click(function () {
                    $(".SCover").hide();
                    $(".glyphicon-ok").hide();
                    $(".SCover", this).show();
                    $(".glyphicon-ok", this).show();
                });
            }
        }
    });
}

function selectMaterial(mediaid) {
    $('#' + mediaid).siblings().attr('curMedia', 0);
    $('#' + mediaid).attr('curMedia', 1);
}
