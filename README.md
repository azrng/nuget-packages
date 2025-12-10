# nuget-package
nuget包仓库,编写常用的代码发布nuget包

## 合并推送脚本

仓库地址

```shell
# gitee
git@gitee.com:AZRNG/nuget-packages.git

# github
git@github.com:azrng/nuget-packages.git
```

推送命令

```shell
# 将github仓库地址添加到origin的推送地址中
git remote set-url --add origin git@github.com:azrng/nuget-packages.git

# 之后只需执行一次推送，即可同步到两个仓库
git push origin master

# 取消配置
git remote set-url --delete origin 仓库地址
```

