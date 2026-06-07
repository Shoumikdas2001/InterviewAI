import { useEffect, useState } from 'react';
import {
  Table, Card, Tag, Typography, Row, Col,
  Statistic, Input, Switch, message, Space,
} from 'antd';
import {
  UserOutlined, SearchOutlined, StopOutlined, CheckCircleOutlined,
} from '@ant-design/icons';
import { adminApi } from '../api/interview.api';
import { dashboardApi } from '../api/interview.api';
import type { AdminDashboard, AdminUser } from '../types';

const { Title, Text } = Typography;

export function AdminPage() {
  const [dashboard, setDashboard] = useState<AdminDashboard | null>(null);
  const [users, setUsers] = useState<AdminUser[]>([]);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState('');

  useEffect(() => {
    Promise.all([
      dashboardApi.getAdminDashboard(),
      adminApi.getUsers(),
    ]).then(([dash, userList]) => {
      setDashboard(dash);
      setUsers(userList);
    }).catch(console.error)
      .finally(() => setLoading(false));
  }, []);

  const handleToggleStatus = async (id: string, currentStatus: boolean) => {
    try {
      await adminApi.toggleUserStatus(id);
      setUsers((prev) => prev.map((u) => u.id === id ? { ...u, isActive: !currentStatus } : u));
      message.success(`User ${currentStatus ? 'deactivated' : 'activated'}`);
    } catch {
      message.error('Failed to update user status');
    }
  };

  const filteredUsers = users.filter((u) =>
    u.fullName.toLowerCase().includes(search.toLowerCase()) ||
    u.email.toLowerCase().includes(search.toLowerCase())
  );

  const columns = [
    {
      title: 'User',
      dataIndex: 'fullName',
      key: 'user',
      render: (name: string, rec: AdminUser) => (
        <div>
          <Text style={{ color: '#fff', fontWeight: 600, display: 'block' }}>{name}</Text>
          <Text style={{ color: 'rgba(255,255,255,0.4)', fontSize: 12 }}>{rec.email}</Text>
        </div>
      ),
    },
    {
      title: 'Status',
      dataIndex: 'isActive',
      key: 'status',
      render: (active: boolean, rec: AdminUser) => (
        <Switch
          checked={active}
          onChange={() => handleToggleStatus(rec.id, active)}
          checkedChildren={<CheckCircleOutlined />}
          unCheckedChildren={<StopOutlined />}
          style={{ background: active ? '#10B981' : undefined }}
        />
      ),
    },
    {
      title: 'Interviews',
      dataIndex: 'totalInterviews',
      key: 'interviews',
      render: (n: number) => <Text style={{ color: '#fff' }}>{n}</Text>,
    },
    {
      title: 'Avg. Score',
      dataIndex: 'averageScore',
      key: 'score',
      render: (score: number) => (
        <Text style={{ color: score >= 7 ? '#10B981' : score >= 5 ? '#F59E0B' : '#EF4444', fontWeight: 600 }}>
          {score ? score.toFixed(1) : '—'}/10
        </Text>
      ),
    },
    {
      title: 'Joined',
      dataIndex: 'createdAt',
      key: 'joined',
      render: (d: string) => <Text style={{ color: 'rgba(255,255,255,0.5)', fontSize: 12 }}>{new Date(d).toLocaleDateString()}</Text>,
    },
  ];

  const cardStyle = {
    background: 'rgba(255,255,255,0.03)',
    border: '1px solid rgba(255,255,255,0.07)',
    borderRadius: 16,
  };

  return (
    <div>
      <Title level={2} style={{ color: '#fff', marginBottom: 32 }}>
        👑 Admin Dashboard
      </Title>

      {dashboard && (
        <Row gutter={[16, 16]} style={{ marginBottom: 32 }}>
          {[
            { title: 'Total Users', value: dashboard.totalUsers, color: '#6C63FF' },
            { title: 'Active Users', value: dashboard.activeUsers, color: '#10B981' },
            { title: 'Total Interviews', value: dashboard.totalInterviews, color: '#48c6ef' },
            { title: 'New This Month', value: dashboard.newUsersThisMonth, color: '#F59E0B' },
          ].map((stat, i) => (
            <Col xs={12} md={6} key={i}>
              <Card style={{ ...cardStyle, borderLeft: `3px solid ${stat.color}` }}>
                <Statistic
                  title={<Text style={{ color: 'rgba(255,255,255,0.5)', fontSize: 12 }}>{stat.title}</Text>}
                  value={stat.value}
                  valueStyle={{ color: '#fff', fontWeight: 700 }}
                />
              </Card>
            </Col>
          ))}
        </Row>
      )}

      {dashboard?.topSkills && (
        <Card style={{ ...cardStyle, marginBottom: 24 }}>
          <Title level={4} style={{ color: '#fff', marginBottom: 16 }}>Top Skills Practiced</Title>
          <Space wrap>
            {dashboard.topSkills.map((s, i) => (
              <Tag key={i} color="purple" style={{ fontSize: 13, padding: '4px 12px' }}>
                {s.skill} ({s.count})
              </Tag>
            ))}
          </Space>
        </Card>
      )}

      {/* User Table */}
      <Card style={cardStyle}>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 20 }}>
          <Title level={4} style={{ color: '#fff', margin: 0 }}>
            <UserOutlined style={{ marginRight: 8 }} />
            Users ({filteredUsers.length})
          </Title>
          <Input
            prefix={<SearchOutlined style={{ color: '#6C63FF' }} />}
            placeholder="Search users..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            style={{
              width: 280,
              background: 'rgba(255,255,255,0.05)',
              border: '1px solid rgba(255,255,255,0.1)',
              borderRadius: 8,
            }}
          />
        </div>
        <Table
          dataSource={filteredUsers}
          columns={columns}
          rowKey="id"
          loading={loading}
          pagination={{ pageSize: 15 }}
          scroll={{ x: 600 }}
        />
      </Card>
    </div>
  );
}
