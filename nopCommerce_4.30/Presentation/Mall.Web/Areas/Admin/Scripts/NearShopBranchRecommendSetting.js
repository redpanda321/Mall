function DeleteRecommendShopBranch(shopbranchId) {
    $.dialog.confirm('确定要删除吗？', function () {
        var loading = showLoading();
        $.post('/admin/NearShopBranch/ResetShopBranchRecommend', { id: shopbranchId }, function (result) {
            loading.close();
            if (result.success) {
                $.dialog.succeedTips("删除成功");
                setTimeout(function () {
                    initRecommendShopBranchTable();
                }, 1500);
            }
            else
                $.dialog.errorTips('删除失败！' + result.msg);
        });
    });
}

$(function () {
    initRecommendShopBranchTable();

    $("#area-selectorRecommend").RegionSelector({
        selectClass: "input-sm select-sort-auto",
        valueHidden: "#AddressIdRecommend",
        maxLevel: 2
    });

    $("#recommendShopBranchTable").on("click", '.glyphicon-circle-arrow-up', function () {
        var oriShopBranchId = parseInt($(this).attr('rowNumber'));
        var rowIndex = parseInt($(this).attr('rowIndex'));
        if (rowIndex > 0) {
            var newShopBranchId = parseInt($('#recommendShopBranchTable .glyphicon-circle-arrow-up[rowIndex="' + (rowIndex - 1) + '"]').attr('rowNumber'));
            changeRecommendSequence(oriShopBranchId, newShopBranchId, function () {
                $("#recommendShopBranchTable").MallDatagrid('reload', {});
            });
        }
    }).on("click", '.glyphicon-circle-arrow-down', function () {
        var oriShopBranchId = parseInt($(this).attr('rowNumber'));
        var rowIndex = parseInt($(this).attr('rowIndex'));
        var nextRow = $('#recommendShopBranchTable .glyphicon-circle-arrow-up[rowIndex="' + (rowIndex + 1) + '"]');
        if (nextRow.length > 0) {
            var newShopBranchId = parseInt(nextRow.attr('rowNumber'));
            changeRecommendSequence(oriShopBranchId, newShopBranchId, function () {
                $("#recommendShopBranchTable").MallDatagrid('reload', {});
            });
        }
    });

    $('#recommendBtn').unbind('click').click(function () {
        $("#recommendGrid").MallDatagrid('reload', getRecommendQuery());
    });
});

function getRecommendQuery() {
    var titleKeyword = $('#titleKeywordRecommend').val();
    var tagsId = $('#tagsIdRecommend').val();
    var addressId = $('#AddressIdRecommend').val();
    var isRecommend = false;
    return { titleKeyword: titleKeyword, tagsId: tagsId, addressId: addressId, isRecommend: isRecommend };
}

function resetRecommendQuery() {
    $("#area-selectorRecommend").RegionSelector("Reset");
    $("#titleKeywordRecommend").val("");
    $("#tagsIdRecommend").val(0);
}

function changeRecommendSequence(oriShopBranchId, newShopBranchId, callback) {
    var loading = showLoading();
    var result = false;
    $.ajax({
        type: 'post',
        url: '/admin/NearShopBranch/RecommendChangeSequence',
        cache: false,
        async: true,
        data: { oriShopBranchId: oriShopBranchId, newShopBranchId: newShopBranchId },
        dataType: "json",
        success: function (data) {
            loading.close();
            if (!data.success)
                $.dialog.tips('调整顺序出错!' + data.msg);
            else
                callback();
        },
        error: function () {
            loading.close();
        }
    });
}

function initRecommendShopBranchTable() {
    //门店表格
    $("#recommendShopBranchTable").MallDatagrid({
        url: '/admin/NearShopBranch/GetRecommendShopBranch',
        nowrap: false,
        rownumbers: true,
        NoDataMsg: '没有找到任何推荐门店',
        border: false,
        fit: true,
        fitColumns: true,
        pagination: false,
        idField: "id",
        pagePosition: 'bottom',
        columns:
        [[
            {
                field: "ShopBranchName", title: '门店名称', align: "center", width: 200
            },
            {
                field: "AddressDetail", title: '门店地址', align: "left", formatter: function (value, row, index) {
                    return value;
                },
            },
            {
                field: "displaySequence", title: '排序', align: "center", width: 80, formatter: function (value, row, index) {
                    return '<span class="glyphicon glyphicon-circle-arrow-up" rownumber="' + row.Id + '" rowIndex="' + index + '"></span> <span class="glyphicon glyphicon-circle-arrow-down" rownumber="' + row.Id + '" rowIndex="' + index + '"></span>';
                }
            },
            {
                field: "Id", title: '操作', align: "center", width: 80, formatter: function (value, row, index) {
                    return ' <span class="btn-a">\
                                    <a class="good-check" onclick="DeleteRecommendShopBranch(' + value + ')">删除</a>\
                                </span>';
                }
            }
        ]],
        onLoadSuccess: function () {
            $("#recommendShopBranchTable tbody tr").first().find('.glyphicon-circle-arrow-up').addClass('disabled');
            $("#recommendShopBranchTable tbody tr").last().find('.glyphicon-circle-arrow-down').addClass('disabled');
        }
    });
}


$('#addRecommendShopBranch').click(function () {
    resetRecommendQuery();
    addRecommendShopBranch();
});

function addRecommendShopBranch() {
    $.dialog({
        title: '推荐门店',
        lock: true,
        padding: '0 5px',
        width: 550,
        id: 'editRecommendShopBranch',
        content: $('#editRecommendShopBranch')[0],
        okVal: '确认',
        ok: function () {
            return submitRecommendShopBranch();
        }
    });

    var prohp = document.location.protocol;//表示当前的网络协议
    var queryUrl = "/admin/NearShopBranch/ShopList";
    var jumpUrl = prohp + "//" + window.location.host + "/m-wap/ShopBranch/Index/{0}";
    $("#recommendGrid").MallDatagrid({
        url: queryUrl,
        nowrap: false,
        rownumbers: true,
        NoDataMsg: '没有找到符合条件的数据',
        border: false,
        fit: true,
        fitColumns: true,
        pagination: true,
        idField: "id",
        pageSize: 10,
        pagePosition: 'bottom',
        pageNumber: 1,
        queryParams: getRecommendQuery(),
        columns:
        [[
            {
                field: "id", title: '选择', align: "center", width: 80, formatter: function (value, row, index) {
                    var html = '<input type="checkbox" name="recommend" sbId="' + value + '" />';
                    return html;
                }
            },
            {
                field: "name", title: '名称', align: "center", formatter: function (value, row, index) {
                    var html = '<a href="' + jumpUrl.replace("{0}", row.id) + '" target="_blank">' + row.name + '</a>';
                    return html;
                }
            },
            {
                field: "tags", title: '标签', align: "center"
            }
        ]]
    });
}

function submitRecommendShopBranch() {
    var returnResult = false;
    var object;
    try {
        var selectedIds = [];
        $('input[name="recommend"]:checked').each(function (i, entity) {
            selectedIds.push($(entity).attr('sbId'));
        });
        if (selectedIds.length <= 0) {
            $.dialog.tips("未推荐任何门店");
            return;
        }
        var loading = showLoading();
        $.ajax({
            type: "post",
            url: '/admin/NearShopBranch/RecommendShopBranch',
            data: { ids: selectedIds },
            dataType: "json",
            async: false,
            success: function (result) {
                loading.close();
                if (result.success) {
                    returnResult = true;
                    $.dialog.tips('保存成功');
                    setTimeout(function () {
                        initRecommendShopBranchTable();
                    }, 1500);
                }
                else
                    $.dialog.tips('保存失败！' + result.msg);
            }
        });
    }
    catch (e) {
        $.dialog.errorTips('保存失败!' + e.message);
    }
    return returnResult;
}

