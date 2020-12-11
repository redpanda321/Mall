//移动端列表loading
//author:DZY[151124]
var h_loading = {
    _dom: $("#autoLoad"),
    init: function (d) {
        this._dom = $(d);
        this.hide();
    },
    show: function () {
        this._dom.empty().html("<span><\/span>");
        this._dom.show();
    },
    hide: function () {
        this._dom.hide();
    },
    nodata: function (text) {
        text = text || "已经没有更多了";
        this._dom.html(text);
    }
};