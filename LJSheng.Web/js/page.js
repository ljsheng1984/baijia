$(document).ready(function() {
	//	商城首页
	$(".shop-type ul li").each(function() {
		$(this).click(function() {
			$(this).addClass("active").siblings().removeClass("active");
		})
	});
	//	订单中心
	$(".order-type ul li").each(function() {
		$(this).click(function() {
			$(this).addClass("active").siblings().removeClass("active");
		})
	});
	//	订单中心-物流状态
	$(".order-status ul li").each(function() {
		$(this).click(function() {
			$(this).addClass("active").siblings().removeClass("active");
		})
	});

	// 商城 商品分类的高度
	var banner_height = $(".shop-banner").height();
	var body_height = $("body").height();
	$(".shop-content").height(body_height - banner_height - 148 + 'px');

	//	显示确认收货确认弹窗
	$(".confirm-goods").click(function() {
		$(".confirm-goods-dialog").addClass("active");
	});
	//	关闭确认收货确认弹窗
	$(".confirm-goods-dialog-btn span").click(function() {
		$(".confirm-goods-dialog").removeClass("active");
	});
});