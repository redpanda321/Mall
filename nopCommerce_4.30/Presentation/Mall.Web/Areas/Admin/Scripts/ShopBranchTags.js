
$(function () {
    $("#divSetLabel .form-group").css({ "width": "150px", "float": "left", "border": "none", "white-space": "nowrap", "overflow": "hidden", "margin": "10px", "text-overflow": "ellipsis" });
});

$(function () {
    query();

    //添加管理员
    $('#btnAddTags').click(function () {
        $.dialog({
            title: '添加标签',
            id: 'addManager',
            content: document.getElementById("addTagsform"),
            lock: true,
            okVal: '保存',
            padding: '0 40px',
            init: function () {
                $("#txtTitle").focus();
                $("#txtTitle").val("");
            },
            ok: function () {
                var title = $("#txtTitle").val();
                AddTag(title);
            }
        });
    });
})


function query() {
    $("#list").MallDatagrid({
        url: './TagList',
        nowrap: false,
        rownumbers: true,
        NoDataMsg: '没有找到符合条件的数据',
        border: false,
        fit: true,
        fitColumns: true,
        pagination: false,
        idField: "Id",
        queryParams: {},
        toolbar: '',
        columns:
        [[
            { field: "Id", hidden: true },
            { field: "Title", title: '标签名称' },
            {
                field: "ShopBranchCount", title: '门店数量', formatter: function (value, row, index) {
                    var html = "<a href=\"/Admin/ShopBranch/management?shopBranchTagId=" + row.Id + "\">" + value + "</a>";
                    return html;
                }
            },
        {
            field: "operation", operation: true, title: "操作",
            formatter: function (value, row, index) {
                var id = row.Id.toString();
                var Title = row.Title.toString();
                var html = ["<span class=\"btn-a\">"];
                html.push("<a onclick=\"ChangeTitle('" + id + "','" + Title + "');\">修改</a>");
                html.push("<a onclick=\"Delete('" + id + "');\">删除</a>");
                html.push("</span>");
                return html.join("");
            }
        }
        ]]
    });
}


function AddTag(title) {
    var loading = showLoading();
    $.ajax({
        type: 'post',
        url: 'AddTag',
        cache: false,
        async: true,
        data: { title: title },
        dataType: "json",
        success: function (data) {
            loading.close();
            if (data.success) {
                $.dialog.tips("添加成功！");
                $("#addTagsform input").val("");
                query();
            }
            else {
                $.dialog.errorTips("添加失败！" + data.msg);
            }
        },
        error: function () {
            loading.close();
        }
    });
}
function Delete(id) {
    $.dialog.confirm('删除此标签，所有门店将会取消此标签，是否继续？', function () {
        var loading = showLoading();
        $.post("./DeleteTag", { id: id }, function (data) {
            loading.close();
            if (data.success) {
                $.dialog.tips("删除成功！");
                query();
            }
            else {
                $.dialog.errorTips("添加失败！" + data.msg);
            }
        });
    });
}
function ChangeTitle(Id, title) {

    $.dialog({
        title: '编辑标签',
        id: 'addManager',
        content: document.getElementById("addTagsform"),
        lock: true,
        okVal: '保存',
        padding: '0 40px',
        init: function () {
            $("#txtTitle").focus();
            $("#txtTitle").val(title);
            $("#txtTagId").val(Id);
        },
        ok: function () {
            var title = $("#txtTitle").val();
            var tagId = $("#txtTagId").val();
            EditTag(tagId, title);
        }
    });
}
function EditTag(id, title) {
    var loading = showLoading();
    $.ajax({
        type: 'post',
        url: 'EditTag',
        cache: false,
        async: true,
        data: { Id: id, title: title },
        dataType: "json",
        success: function (data) {
            loading.close();
            if (data.success) {
                $.dialog.tips("修改成功！");
                $("#addTagsform input").val("");
                query();
            }
            else {
                $.dialog.errorTips("修改失败！" + data.msg);
            }
        },
        error: function () {
            loading.close();
        }
    });
}
