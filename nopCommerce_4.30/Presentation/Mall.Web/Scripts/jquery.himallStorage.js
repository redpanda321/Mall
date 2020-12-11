//本地存储，localStorage类没有存储空间的限制
window.MallStorage = (new (function () {

	var storage = localStorage;    //声明一个变量，用于确定使用哪个本地存储函数  

	this.setItem = function (key, value) {
		storage.setItem(key, value);
	};

	this.getItem = function (name) {
		return storage.getItem(name);
	};

	this.removeItem = function (key) {
		storage.removeItem(key);
	};

	this.clear = function () {
		storage.clear();
	};
})());