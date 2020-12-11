function HtmlDecode(str) {
    var t = document.createElement("div");
    t.innerHTML = str;
    return t.innerText || t.textContent
}
var currentId = 0;
$(function () {
    $('.complain-btn').click(function () {
        currentId = $(this).attr("cid");
        var type = $(this).attr("deal");
        var replyContent = myHTMLDeCode($(this).parent().find("span").html());
        switch (type) {
            case "cancel":
                $.dialog.confirm("取消此次投诉！", function () { DealComplaint() });
                break;
            case "ok":
                $.dialog({
                    title: '对投诉结果满意',
                    lock: true,
                    width: 400,
                    id: 'Agree',
                    content: '<p class="ftx03">商家回复：' + replyContent + '</p><br />是否满意商家的回复？',
                    padding: '20px',
                    cancelVal: '取消',
                    ok: function () {
                        DealComplaint();
                    },
                    cancel: true
                });
                break;
            case "bad":
                $.dialog({
                    title: '申请仲裁',
                    lock: true,
                    width: 400,
                    id: 'goodCheck',
                    content: '<p class="ftx03">商家回复：' + replyContent + '</p><br />是否不满意商家的回复并且进行投诉？',
                    padding: '20px',
                    cancelVal: '取消',
                    ok: function () {
                        ApplyArbitration();
                    },
                    cancel: true
                });
                break;
        }
    });
});
function DealComplaint() {
    var loading = showLoading();
    $.ajax({
        type: 'post',
        url: '/OrderComplaint/DealComplaint',
        cache: false,
        data: { id: currentId },
        dataType: 'json',
        success: function (data) {
            loading.close();
            if (data.success) {
                $.dialog.succeedTips("处理成功！", function () {
                    window.location.href = window.location.href;
                }, 1);
            }
        },
        error: function () { }
    });
}
function myHTMLEnCode(str) {
    var s = "";
    if (!str || str.length == 0) return "";
    s = str.replace(/&amp;/g, "&");
    s = s.replace(/&lt;/g, "<");
    s = s.replace(/&gt;/g, ">");
    s = s.replace(/ /g, "&nbsp;");
    s = s.replace(/\'/g, "‘");
    s = s.replace(/\"/g, "“");
    return s;
}

function myHTMLDeCode(str) {
    var s = "";
    if (str.length == 0) return "";
    s = str.replace(/&amp;/g, "&");
    s = s.replace(/&lt;/g, "<");
    s = s.replace(/&gt;/g, ">");
    s = s.replace(/&nbsp;/g, " ");
    s = s.replace(/&#39;/g, "\'");
    s = s.replace(/&quot;/g, "\"");
    s = s.replace(/<br>/g, "\n");
    return s;
}
function ApplyArbitration() {
    var loading = showLoading();
    $.ajax({
        type: 'post',
        url: '/OrderComplaint/ApplyArbitration',
        cache: false,
        data: { id: currentId },
        dataType: 'json',
        success: function (data) {
            loading.close();
            if (data.success) {
                $.dialog.succeedTips("申请成功！", function () {
                    window.location.href = window.location.href;
                }, 1);
            }
        },
        error: function () { }
    });
}

function OpenComplaintReason(orderId, complaintReason, sellerReply, platRemark) {
    $.dialog({
        title: '查看原因',
        lock: true,
        width: 550,
        id: 'goodCheck',
        content: ['<div class="dialog-form">',
            '<div class="form-group">',
            '<label class="label-inline fl">订单号</label>',
            '<p class="only-text">' + orderId + '</p>',
            '</div>',
            '<div class="form-group">',
            '<label class="label-inline fl">投诉原因</label>',
            '<p class="only-text">' + myHTMLDeCode(complaintReason) + '&nbsp;</p>',
            '</div>',
            '<div class="form-group">',
            '<label class="label-inline fl">商家回复</label>',
            '<p class="only-text">' + myHTMLDeCode(sellerReply) + '&nbsp;</p>',
            '</div>',
            '<div class="form-group">',
            '<label class="label-inline fl">平台备注</label>',
            '<p class="only-text">' + myHTMLDeCode(platRemark) + '&nbsp;</p>',
            '</div>',
            '</div>'].join(''),
        padding: '0 40px',
        okVal: '确定',
        ok: function () {
        }
    });
}