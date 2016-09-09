using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using IS3.Core;

namespace IS3.SimpleStructureTools.Helper.Analysis
{
    public class LoadStructure : DGObject
    {
        public double radius { get; set; }
        public double thickness { get; set; }
        public double width { get; set; }
        public double density { get; set; }
        public double moe { get; set; }
        public double pr { get; set; }
        public double k_ground { get; set; }
        public double k_joint { get; set; }
        public double pv { get; set; }   //隧道顶部均布荷载值
        public double ph1 { get; set; }  //侧向水土压力，梯形荷载的上部值（kPa）
        public double ph2 { get; set; }  //侧向水土压力，梯形荷载的下部值（kPa）
        public string strResult { get; set; }
        public List<NodeResult> Result { get; set; }
        public LoadStructure()
        {
            Result = null;
        }

        public class NodeResult
        {
            public int n { get; set; }
            public double x { get; set; }
            public double y { get; set; }
            public double moment { get; set; }
            public double axial { get; set; }
            public double shear { get; set; }
        }
        public static string createCode(LoadStructure loadStructure)
        {
            string str = string.Format(@"/clear
/Title,Analysis on the segment lining of ChuanChang Road Section
/prep7
*afun,rad
pi=acos(-1.0)
! 定义变量 （不同的隧道据实际情况确定变量值）
r0={0}                                           ! 管片半径（m）
t_c={1}                                       ! 管片厚度（m）
b_c={2}                                         ! 管片宽度（m）
I_c=t_c**3*b_c/12                       ! 管片截面惯性矩（m^3）
E_c={3}                                   ! C55混凝土弹性模型(kPa)
u_c={4}                                        ! 混凝土泊松比
dens_c={5}                                   ! 混凝土密度（10^3 kg/m^3）
k_ground={6}                             ! 地层弹簧刚度（kN/m^3）
k_small=1                                      ! 表面效应单元surf153，消除刚体位移。
!kn=3e6                                           ! 接头弹簧的轴向刚度（kN/m^3）
!ks=3e6                                           ! 接头弹簧的剪切刚度（kN/m^3）
km={7}                                      ! 接头弹簧的转动刚度（kN*m/rad）

! 定义荷载值
g=10                                               ! 重力加速度（N/kg）
! 隧道顶部均布荷载值。覆土深度9.2m，土体的重度取为18.3，上部超载取为20kPa （kPa）  
PV={8:0.00}                                    
PH1={9:0.00}                                   ! 侧向水土压力，梯形荷载的上部值（kPa）
PH2={10:0.00}                                    ! 侧向水土压力，梯形荷载的下部值（kPa）
PR=PV+pi*dens_c*g*t_c                ! 拱底反力 （kPa）

! 定义接头位置
joint_num=6                                    ! 6个接头
*dim,joint_loc,array,6
*dim,joint_node1,array,6
*dim,joint_node2,array,6
*dim,joint_element,array,6
joint_loc(1)=8                                   ! 第一个接头在8°处，y轴开始记度数，逆时针方向
joint_loc(2)=joint_loc(1)+65             ! 第二个接头在73°处
joint_loc(3)=joint_loc(2)+65             ! 第三个接头在138°处
joint_loc(4)=joint_loc(3)+84             ! 第四个接头在222°处
joint_loc(5)=joint_loc(4)+65             ! 第五个接头在287°处
joint_loc(6)=joint_loc(5)+65             ! 第六个接头在352°处

! 定义单元划分数量
num_element=360                                 ! 划分成360个单元
angle_div=360/num_element                  ! 每个单元角度，1°
len_element= 2*pi*r0/num_element    ! 每个单元的长度
k_link10=k_ground*len_element           ! 等效的作用在节点上的地层弹簧刚度

! 定义材料
et,1,beam188                                          ! 定义衬砌管片，beam188梁单元
! tb 可以用来设置材料的本构模型（包括弹塑蠕本构）也许对于混凝土应该采用双折线模型biso
mp,ex,1,E_c                                             ! 梁弹性模量
mp,prxy,1,u_c                                          ! 泊松比
mp,dens,1,dens_c                                   ! 密度
sectype,1,beam,rect                                ! 定义梁的截面形式是矩形
secdata,t_c,b_c                                        ! 定义矩形截面的宽度和高度

et,2,link10                                                ! 定义地层弹簧，link10
keyopt,2,3,1                                              ! 定义地层弹簧只受压（compression-only）
mp,ex,2,k_link10                                      ! 定义弹簧的刚度
r,2,1                                                           ! 定义实常数，这里指的是弹簧的截面面积是1

et,3,combin39                                            ! 定义接头弹簧类型，非线性弹簧
keyopt,3,3,6                                                ! 绕Z轴转动的弹簧
! 转动弹簧在正负弯矩下的转动刚度（需根据实际情况改正）force-deflection curve。正弯矩下转动刚度是负弯矩的2-3倍
!r,3,-0.002,-100,0,0,0.002,50
r,3,-0.04,-200,-6.67e-3,-100,-3.33e-3,-50
rmore,0.0,0.0,0.0107,80,0.064,160


et,4,surf153                                                  ! 消除刚体位移的平面单元
keyopt,4,4,1                                                  ! 没有中间节点
mp,dens,4,0
r,4,,,,k_small                                                 ! 定义很小的基础刚度消除刚体位移
                      
! 创建梁节点
csys,1                                                            ! 总体柱坐标系，原点在（0,0,0）处
*do,i,0,360-angle_div,angle_div                  ! 循环，每1°一个节点，共360个
    n,,r0,i+90
*enddo
*get,max_beam_node,NODE,0,NUM,MAX   ! 选出最大节点数

! 创建地层节点
*do,i,0,360-angle_div,angle_div
    n,,r0+1,i+90
    *get,max_node,NODE,0,NUM,MAX
    d,max_node,all,0
*enddo                                                            ! 固定住每个节点

! 接头节点
*do,i,1,joint_num
joint_node1(i)=joint_loc(i)/angle_div+1
n,,r0,joint_loc(i)+90
*get,joint_node2(i),node,o,num,max
*enddo

! 梁单元
type,1
real,1
mat,1
*do,i,1,joint_node1(1)-1
e,i,i+1
*enddo                                                           ! 管片单元第一段

*do,i,1,joint_num-1
node1=joint_node1(i)
node2=joint_node1(i+1)
node101=joint_node2(i)		 
e,node101,node1+1                                   ! 接头单元
*do,j,node1+1,node2-1
e,j,j+1
*enddo
*enddo

node101=joint_node2(joint_num)
e,node101,node2+1
*do,i,node2+1,max_beam_node-1
  e,i,i+1
*enddo
e,max_beam_node,1

! 地层单元
type,2
real,2
mat,2
*do,i,1,num_element
e,i,i+num_element
*enddo
! 接头单元
type,3
real,3
mat,3
*do,i,1,joint_num
e,joint_node1(i),joint_node2(i)
*get,joint_element(i),elem,0,num,max
cp,i*3+1,ux,joint_node1(i),joint_node2(i)   ! x方向位移一致
cp,i*3+2,uy,joint_node1(i),joint_node2(i)    ! y方向位移一致
*enddo
! 表面效应单元
type,4
real,4
mat,4
*do,i,1,360          ! 施加表面效应单元
node1=nelem(i,1)
node2=nelem(i,2)
e,node1,node2
*enddo


! 约束条件
d,all,uz,0
d,all,rotx,0
d,all,roty,0
acel,0,g,0                                                    ! 施加重力
! 等效节点荷载
! 左侧水土压力
csys,0
fcum,add
*do,i,num_element*2+joint_num+1,num_element*2+joint_num+180,1
node1=nelem(i,1)
node2=nelem(i,2)
y1=ny(node1)
y2=ny(node2)
f1=PH1+(PH2-PH1)*(r0-y1)/(2*r0)
f2=PH1+(PH2-PH1)*(r0-y2)/(2*r0)
fn1=f1*(y1-y2)/2+(f2-f1)*(y1-y2)/6
fn2=f1*(y1-y2)/2+(f2-f1)*(y1-y2)/3
fcum,add
f,node1,fx,fn1
f,node2,fx,fn2
*enddo


! 右侧水土压力
*do,i,num_element*2+joint_num+181,num_element*2+joint_num+360,1
node1=nelem(i,1)
node2=nelem(i,2)
y1=ny(node1)
y2=ny(node2)
f1=PH1+(PH2-PH1)*(r0-y1)/(2*r0)
f2=PH1+(PH2-PH1)*(r0-y2)/(2*r0)
fn1=f2*(y1-y2)/2+(f1-f2)*(y1-y2)/3
fn2=f2*(y1-y2)/2+(f1-f2)*(y1-y2)/6
fcum,add
f,node1,fx,fn1
f,node2,fx,fn2
*enddo

! 上部荷载
*do,i,num_element*2+joint_num+1,num_element*2+joint_num+90,1
node1=nelem(i,1)
node2=nelem(i,2)
x1=nx(node1)
x2=nx(node2)
fn1=PV*(x2-x1)/2
fn2=fn1
fcum,add
f,node1,fy,fn1
f,node2,fy,fn2
*enddo

*do,i,num_element*2+joint_num+271,num_element*2+joint_num+360,1
node1=nelem(i,1)
node2=nelem(i,2)
x1=nx(node1)
x2=nx(node2)
fn1=PV*(x2-x1)/2
fn2=fn1
fcum,add
f,node1,fy,fn1
f,node2,fy,fn2
*enddo

! 拱底反力
*do,i,num_element*2+joint_num+91,num_element*2+joint_num+270,1
node1=nelem(i,1)
node2=nelem(i,2)
x1=nx(node1)
x2=nx(node2)
fn1=PR*(x2-x1)/2
fn2=fn1
fcum,add
f,node1,fy,fn1
f,node2,fy,fn2
*enddo

allsel,all
/solu
deltim,0.01
lnsrch,on
solve
save

/post1

esel,s,elem,,1,num_element
etable,m1,smisc,3
etable,m2,smisc,16
plls,m1,m2,-1                                                 ! 弯矩图

etable,n1,smisc,1                                           ! 轴力图
etable,n2,smisc,14
plls,n1,n2,-1

etable,q1,smisc,6                                             ! 剪力图
etable,q2,smisc,19
plls,q1,q2,-1

*dim,m,,360,1
*vget,m(1),elem,1,etab,m1

*dim,n,,360,1
*vget,n(1),elem,1,etab,n1

*dim,q,,360,1
*vget,q(1),elem,1,etab,q1

*dim,x,,360,1
*dim,y,,360,1
*dim,z,,360,1

*do,i,1,num_element
*if,NSEL(i),gt,0,then
x(i,1)=UX(i)
y(i,1)=UY(i)
z(i,1)=UZ(i)
*endif
*enddo

*cfopen,result,txt
*vwrite,x(1),y(1),z(1),m(1),n(1),q(1)
(3(f17.10,1x),3(f17.10,1x))
*CFCLOSE", loadStructure.radius, loadStructure.thickness, loadStructure.width, loadStructure.moe, loadStructure.pr, loadStructure.density / 1000, loadStructure.k_ground, loadStructure.k_joint,
         loadStructure.pv, loadStructure.ph1, loadStructure.ph2);

            return str;
        }
    }
}