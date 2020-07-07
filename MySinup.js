const app = getApp();
Page({
  data: {
    datalist:[],
    count:0,
    path:'',
    Site:'',
    Oid:'',
    out_trade_no: '',
    of_type: '',
    total_fees: 0.0,
    text:'无需支付!您当前没有可支付的费用',
    paymentbol:true,//控制底部支付模板
    moneys:0.0,//统一报名费
    getpayment:false //默认无已缴费数据 隐藏已缴费模板
  },
  Getpaid() {
    var that = this
    wx.showLoading({
      title: '加载中...',
    });
    //查询已缴费
    wx.request({
      url: app.globalData.siteUrl + 'My/paid',
      data: { Oid: that.data.Oid },
      success: function (res) {
        wx.hideLoading();
        //console.log(res.data);
        that.setData({
          datalist: res.data,
          getpayment:true //有数据 打开显示已缴费模板
        });

      }, fail: function () {
        wx.hideLoading();
        wx.showToast({
          title: '哎呀,貌似出错了...',
          icon: 'none',
          duration: 2000//持续的时间
        })
      }
    });
    //延迟读取头像
    setTimeout(function () {
      if (that.data.datalist.notdata=="无数据"){
        return;
      }
      wx.request({
        url: app.globalData.siteUrl + 'company/GetSchoolById',
        data: { SchoolId: that.data.datalist.SchoolId },
        success: function (res) {
          that.setData({
            path: res.data.SchoolPortrait
          });
        }, fail: function () {
          wx.showToast({
            title: '哎呀,貌似出错了...',
            icon: 'none',
            duration: 2000//持续的时间
          })
        }
      });
    }, 500)
  },
  GetUnpaid(){
    var that = this
    wx.showLoading({
      title: '加载中...',
    });
    wx.request({
      url: app.globalData.siteUrl +'School/GetSignupMoney',
      success:function(suc){
        //console.log(suc);
        //保存统一报名费
        that.setData({
          moneys: suc.data[0].Moneys,
          getpayment:false
        });
        //查询未缴费
        wx.request({
          url: app.globalData.siteUrl + 'My/Unpaid',
          data: { Oid: that.data.Oid },
          success: function (res) {
            wx.hideLoading();
            // console.log(res.data);
            that.setData({
              datalist: res.data,
              count: res.data.length
            });
            if (res.data.length == 0) {
              that.setData({
                text: '您还没有可支付的院校费用!'
              });
            } else {
              that.setData({
                text: `您好,请尽快支付${that.data.count * that.data.moneys}元考试费用,费用将用于考试报名!`
              });
            }
          }, fail: function () {
            wx.hideLoading();
            wx.showToast({
              title: '哎呀,貌似出错了...',
              icon: 'none',
              duration: 2000//持续的时间
            })
          }
        });
        //延迟读取头像
        setTimeout(function () {
          var arr = new Array();
          for (var i = 0; i < that.data.datalist.length; i++) {
            wx.request({
              url: app.globalData.siteUrl + 'company/GetSchoolById',
              data: { SchoolId: that.data.datalist[i].SchoolId },
              success: function (res) {
                arr.push(res.data.SchoolPortrait);
                that.setData({
                  path: arr
                });
              }, fail: function () {
                wx.showToast({
                  title: '哎呀,貌似出错了...',
                  icon: 'none',
                  duration: 2000//持续的时间
                })
              }
            });
          }
        }, 500)
      }, fail: function (errs) {
        console.log(errs);
        wx.showToast({
          title: '哎呀,貌似出错了...',
          icon: 'none',
          duration: 2000//持续的时间
        })
      }
    })
  },
  /**
   * 生命周期函数--监听页面加载
   */
  onLoad: function (options) {
    this.setData({
      Site: app.globalData.siteUrl,
      Oid: options.Oid
    });
    app.Jurisdiction();
    //加载未缴费
    this.GetUnpaid();
    
  },
  onClick(event) {
    var title = event.detail.title;
    var that=this;
    //延迟设置标题
    setTimeout(function () {
      wx.setNavigationBarTitle({
        title: "我的报名-" + title
      });
    }, 300)
    that.setData({
      count: 0,
      text: '您还没有可支付的院校费用!'
    });

    if (title=='已缴费'){
      that.Getpaid();
      that.setData({
        paymentbol: false
      })
    }else{
      that.GetUnpaid();
      that.setData({
        paymentbol: true
      })
    }
  },
  /**
  *支付 (that.data.count*111)*100
  *注意 传入余额为分 所有 余额*100 为元
   */
  onSubmit: function () {
    var that = this;
    var bymoney = that.data.count * that.data.moneys;
    if(that.data.count==0){
      wx.showToast({
        title: '无需支付!您当前没有可支付的费用',
        icon: 'none',
        duration: 2000//持续的时间
      })
      return;
    }
    console.log(that.data.datalist);
    console.log('开始请求,oid为:', that.data.Oid);
    // return;
    wx.request({
      url: app.globalData.siteUrl + 'commodity/UnifiedOrder?total_fee=' + bymoney*100 + '&OpenId=' + that.data.Oid + '',
        success: function (data) {
          var a = JSON.parse(data.data.json)
          console.log(data,a);
          that.setData({
            out_trade_no: data.data.out_trade_no,
            total_fees: data.data.total_fees,
          })
          console.log('开始发起微信支付...');
          wx.requestPayment({
            timeStamp: data.data.timeStamp,
            nonceStr: data.data.nonceStr,
            package: 'prepay_id=' + a.xml.prepay_id.cdatasection + '',
            signType: 'MD5',
            paySign: data.data.paySign,
            success(res) {
              if (res.errMsg == "requestPayment:ok") {
                // console.log(that.data.out_trade_no);
                //延迟
                setTimeout(function () {
                 wx.request({
                   url: app.globalData.siteUrl +'My/SetPayment',
                   data:{
                     Tid: that.data.datalist[0].Id,
                     out_trade_no: that.data.out_trade_no,
                     total_fee: that.data.total_fees
                   },
                   success:function(ress){
                     console.log(ress);
                     if(ress.data==1){
                       that.GetUnpaid();
                     }
                   },
                   fail: function (ress) {
                     console.log(ress);
                     wx.showToast({
                       title: '哎呀,貌似出错了...',
                       icon: 'none',
                       duration: 2000//持续的时间
                     })
                   }
                 })
                }, 500)
                wx.showToast({
                  title: '支付成功',
                  duration: 2000//持续的时间
                });
              } else {
                wx.showToast({
                  title: '取消支付',
                  icon: 'none',
                  duration: 2000//持续的时间
                })
              }

            },
            fail(res) {
              console.log('错误:', res)
              wx.showToast({
                title: '哎呀,貌似出错了...',
                icon: 'none',
                duration: 2000//持续的时间
              })
            }
          })
        },
      })
  },
  //点击订单号把订单号复制到粘贴板
  copyText: function (e) {
  //console.log(e)
    wx.setClipboardData({
      data: e.currentTarget.dataset.text,
      success: function (res) {
        wx.getClipboardData({
          success: function (res) {
            wx.showToast({
              title: '已将订单编号复制到粘贴板',
              duration: 2000,//持续的时间
              icon:'none'
            });
          }
        })
      }
    })
  },

  /**
   * 生命周期函数--监听页面初次渲染完成
   */
  onReady: function () {

  },

  /**
   * 生命周期函数--监听页面显示
   */
  onShow: function () {

  },

  /**
   * 生命周期函数--监听页面隐藏
   */
  onHide: function () {

  },

  /**
   * 生命周期函数--监听页面卸载
   */
  onUnload: function () {

  },

  /**
   * 页面相关事件处理函数--监听用户下拉动作
   */
  onPullDownRefresh: function () {

  },

  /**
   * 页面上拉触底事件的处理函数
   */
  onReachBottom: function () {

  },

  /**
   * 用户点击右上角分享
   */
  onShareAppMessage: function () {

  }
})