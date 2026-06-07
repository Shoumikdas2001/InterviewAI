import { useState } from 'react';
import { Outlet, useNavigate, useLocation } from 'react-router-dom';
import { Layout, Menu, Avatar, Dropdown, Badge, Button, Typography, Space } from 'antd';
import {
  DashboardOutlined, FileTextOutlined, VideoCameraOutlined,
  HistoryOutlined, RocketOutlined, LogoutOutlined,
  UserOutlined, BellOutlined, MenuFoldOutlined, MenuUnfoldOutlined,
  CrownOutlined,
} from '@ant-design/icons';
import { useAuth } from '../services/AuthContext';

const { Sider, Header, Content } = Layout;
const { Text } = Typography;

const navItems = [
  { key: '/dashboard', icon: <DashboardOutlined />, label: 'Dashboard' },
  { key: '/resume', icon: <FileTextOutlined />, label: 'Resume' },
  { key: '/interview/new', icon: <VideoCameraOutlined />, label: 'New Interview' },
  { key: '/interview/history', icon: <HistoryOutlined />, label: 'History' },
  { key: '/roadmap', icon: <RocketOutlined />, label: 'Roadmap' },
];

export function MainLayout() {
  const [collapsed, setCollapsed] = useState(false);
  const { user, logout } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();

  const selectedKey = navItems.find((item) =>
    location.pathname.startsWith(item.key)
  )?.key || '/dashboard';

  const userMenuItems = [
    {
      key: 'profile',
      icon: <UserOutlined />,
      label: 'My Profile',
      onClick: () => navigate('/dashboard'),
    },
    ...(user?.role === 'Admin'
      ? [{ key: 'admin', icon: <CrownOutlined />, label: 'Admin Panel', onClick: () => navigate('/admin') }]
      : []),
    { type: 'divider' as const },
    {
      key: 'logout',
      icon: <LogoutOutlined />,
      label: 'Logout',
      danger: true,
      onClick: async () => { await logout(); navigate('/login'); },
    },
  ];

  return (
    <Layout style={{ minHeight: '100vh' }}>
      <Sider
        collapsible
        collapsed={collapsed}
        onCollapse={setCollapsed}
        trigger={null}
        width={240}
        style={{
          background: 'linear-gradient(180deg, #0f0c29 0%, #1a1650 50%, #24243e 100%)',
          boxShadow: '2px 0 20px rgba(108, 99, 255, 0.15)',
          position: 'fixed',
          height: '100vh',
          left: 0,
          top: 0,
          bottom: 0,
          zIndex: 100,
        }}
      >
        {/* Logo */}
        <div style={{
          padding: collapsed ? '20px 16px' : '20px 24px',
          borderBottom: '1px solid rgba(255,255,255,0.07)',
          display: 'flex',
          alignItems: 'center',
          gap: 12,
        }}>
          <div style={{
            width: 36, height: 36,
            background: 'linear-gradient(135deg, #6C63FF, #48c6ef)',
            borderRadius: 10,
            display: 'flex', alignItems: 'center', justifyContent: 'center',
            fontSize: 18, color: '#fff', fontWeight: 800, flexShrink: 0,
          }}>
            AI
          </div>
          {!collapsed && (
            <Text style={{ color: '#fff', fontWeight: 700, fontSize: 18, letterSpacing: '-0.3px' }}>
              InterviewAI
            </Text>
          )}
        </div>

        <Menu
          theme="dark"
          selectedKeys={[selectedKey]}
          mode="inline"
          items={navItems}
          onClick={({ key }) => navigate(key)}
          style={{
            background: 'transparent',
            border: 'none',
            marginTop: 8,
          }}
        />

        {/* Collapsed toggle */}
        <div style={{
          position: 'absolute', bottom: 24, width: '100%',
          display: 'flex', justifyContent: 'center',
        }}>
          <Button
            type="text"
            onClick={() => setCollapsed(!collapsed)}
            icon={collapsed ? <MenuUnfoldOutlined /> : <MenuFoldOutlined />}
            style={{ color: 'rgba(255,255,255,0.5)' }}
          />
        </div>
      </Sider>

      <Layout style={{ marginLeft: collapsed ? 80 : 240, transition: 'margin 0.2s' }}>
        <Header style={{
          background: 'rgba(255,255,255,0.02)',
          backdropFilter: 'blur(20px)',
          borderBottom: '1px solid rgba(108, 99, 255, 0.1)',
          padding: '0 24px',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'flex-end',
          position: 'sticky',
          top: 0,
          zIndex: 99,
          gap: 16,
        }}>
          <Badge count={0} dot>
            <Button
              type="text"
              icon={<BellOutlined />}
              style={{ color: 'rgba(255,255,255,0.6)', fontSize: 18 }}
            />
          </Badge>

          <Dropdown menu={{ items: userMenuItems }} placement="bottomRight" arrow>
            <Space style={{ cursor: 'pointer', gap: 10 }}>
              <Avatar
                style={{ background: 'linear-gradient(135deg, #6C63FF, #48c6ef)', cursor: 'pointer' }}
                size={36}
              >
                {user?.firstName?.[0]}{user?.lastName?.[0]}
              </Avatar>
              <div style={{ textAlign: 'left', lineHeight: 1.2 }}>
                <Text style={{ color: '#fff', fontWeight: 600, fontSize: 13, display: 'block' }}>
                  {user?.firstName} {user?.lastName}
                </Text>
                <Text style={{ color: 'rgba(255,255,255,0.4)', fontSize: 11 }}>
                  {user?.role}
                </Text>
              </div>
            </Space>
          </Dropdown>
        </Header>

        <Content style={{ padding: '32px', minHeight: 'calc(100vh - 64px)' }}>
          <Outlet />
        </Content>
      </Layout>
    </Layout>
  );
}
