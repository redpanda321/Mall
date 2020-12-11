$(function () {
    $('#searchButton').click(function (e) {
        reloadDataGrid();
    });
    LoadData();
});
function NameSearch(evt) {
    evt = evt ? evt : window.event;
    if (evt.keyCode == 13) {
        $('#searchButton').click();
        return false;
    }
}
function GetQueryPara()
{
    var para = {};
    para.ShopBranchId = GetQueryString("shopBranchId");
    para.KeyWords = $.trim($('#shopBranchName').val());
    var cid = $('#shopCategory').val();
    if (cid > 0) para.ShopCategoryId = cid;
    return para;
}
function reloadDataGrid()
{
    var para = GetQueryPara();
    $("#shopDatagrid").MallDatagrid('clearReload', para);
}
function LoadData()
{
    var para = {};
    para.ShopBranchId = GetQueryString("shopBranchId");
    $("#shopDatagrid").MallDatagrid({
        url: 'ProductList',
        nowrap: false,
        rownumbers: true,
        NoDataMsg: '没有找到符合条件的门店',
        border: false,
        fit: true,
        fitColumns: true,
        pagination: true,
        idField: "Id",
        pageSize: 10,
        pagePosition: 'bottom',
        pageNumber: 1,
        queryParams: para,
        operationButtons: "#saleOff",
        columns:
        [[
            { checkbox: true, width: 40 },
            {
                field: "name", title: '商品', align: 'left',
                formatter: function (value, row, index) {
                    var html = '<img class="ml15 mr10 fl" width="40" height="40" src="' + row.imgUrl + '" /><a class="single-ellipsis w350 h40 lh40" title="' + value + '" href="/m-wap/branchproduct/detail/' + row.id + '?shopBranchId=' + row.shopBranchId + '" target="_blank">';
                    if (row.ProductType == 1) {
                        html = html + '<span class="virtualMark">(虚拟)</span>';
                    }
                    html+= value + '</a>';
                    return html;
                }
            },
            { field: "categoryName", title: "商家分类", width: 90, align: "center" },
        {
            field: "price", title: "门店价格", width: 150, align: "center",
            formatter: function (value, row, index) {
                var html = row.price.toFixed(2);
                if(row.MinPrice != row.MaxPrice){
                    html = row.MinPrice.toFixed(2) + '-' + row.MaxPrice.toFixed(2);
                }
                return html;
            }
        },
        {
            field: "saleCounts", title: "销量", width: 150, align: "center"
        },
        { field: "stock", title: "门店库存", width: 90, align: "center" }
        ]],
        onLoadSuccess: function () {
        }
    });
}