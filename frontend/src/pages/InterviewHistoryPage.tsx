import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Table, Card, Tag, Button, Typography, Select, Input,
  Space, Progress, Tooltip, Empty, Spin,
} from 'antd';
import {
  PlayCircleOutlined, EyeOutlined, SearchOutlined,
  ClockCircleOutlined, TrophyOutlined,
} from '@ant-design/icons';
import { interviewApi } from '../api/interview.api';
import type { InterviewSession } from '../types';

const { Title, Text } = Typography;
const { Option } = Select;

export function InterviewHistoryPage() {
  const [sessions, setSessions] = useState<InterviewSession[]>([]);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState<string>('All');
  const navigate = useNavigate();

  useEffect(() => {
    interviewApi.getHistory(1, 50)
      .then(setSessions)
      .catch(console.error)
      .finally(() => setLoading(false));
  }, []);

  const filtered = sessions.filter((s) => {
    const matchSearch = s.jobRole.toLowerCase().includes(search.toLowerCase()) ||
      s.title?.toLowerCase().includes(search.toLowerCase());
    const matchStatus = statusFilter === 'All' || s.status === statusFilter;
    return matchSearch && matchStatus;
  });

  const columns = [
    {
      title: 'Interview',
      key: 'title',
      render: (_: unknown, rec: InterviewSession) => (
        <div>
          <Text style={{ color: '#fff', fontWeight: 600 }}>{rec.title || rec.jobRole}</Text>
          <Space style={{ marginTop: 4 }} size={4} wrap>
            <Tag color="purple">{rec.interviewType}</Tag>
            <Tag>{rec.difficulty}</Tag>
            {rec.skills?.slice(0, 2).map((s, i) => (
              <Tag key={i} color="blue">{s}</Tag>
            ))}
          </Space>
        </div>
      ),
    },
    {
      title: 'Status',
      dataIndex: 'status',
      key: 'status',
      render: (status: string) => (
        <Tag color={
          status === 'Completed' ? 'green' :
          status === 'InProgress' ? 'blue' :
          status === 'Abandoned' ? 'red' : 'default'
        }>
          {status}
        </Tag>
      ),
    },
    {
      title: 'Score',
      dataIndex: 'overallScore',
      key: 'score',
      sorter: (a: InterviewSession, b: InterviewSession) => (a.overallScore ?? 0) - (b.overallScore ?? 0),
      render: (score: number | undefined) => {
        if (score === undefined) return <Text style={{ color: 'rgba(255,255,255,0.3)' }}>—</Text>;
        const color = score >= 7 ? '#10B981' : score >= 5 ? '#F59E0B' : '#EF4444';
        return (
          <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
            <Text style={{ color, fontWeight: 700 }}>{score?.toFixed(1)}/10</Text>
            <Progress
              percent={score * 10}
              strokeColor={color}
              trailColor="rgba(255,255,255,0.06)"
              showInfo={false}
              style={{ width: 60 }}
              size="small"
            />
          </div>
        );
      },
    },
    {
      title: 'Date',
      dataIndex: 'createdAt',
      key: 'date',
      sorter: (a: InterviewSession, b: InterviewSession) =>
        new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime(),
      render: (d: string) => (
        <Text style={{ color: 'rgba(255,255,255,0.5)', fontSize: 13 }}>
          <ClockCircleOutlined style={{ marginRight: 4 }} />
          {new Date(d).toLocaleDateString()}
        </Text>
      ),
    },
    {
      title: 'Actions',
      key: 'actions',
      render: (_: unknown, rec: InterviewSession) => (
        <Space>
          {rec.status === 'Completed' ? (
            <Tooltip title="View Results">
              <Button
                type="primary"
                icon={<EyeOutlined />}
                size="small"
                style={{ background: 'linear-gradient(135deg, #6C63FF, #48c6ef)', border: 'none' }}
                onClick={() => navigate(`/interview/${rec.id}/results`)}
              >
                Results
              </Button>
            </Tooltip>
          ) : rec.status === 'Created' || rec.status === 'InProgress' ? (
            <Tooltip title="Continue Interview">
              <Button
                icon={<PlayCircleOutlined />}
                size="small"
                type="default"
                onClick={() => navigate(`/interview/${rec.id}/room`)}
              >
                Continue
              </Button>
            </Tooltip>
          ) : null}
        </Space>
      ),
    },
  ];

  const stats = {
    total: sessions.length,
    completed: sessions.filter((s) => s.status === 'Completed').length,
    avgScore: sessions.filter((s) => s.overallScore !== undefined).length > 0
      ? sessions.filter((s) => s.overallScore !== undefined)
          .reduce((acc, s) => acc + (s.overallScore ?? 0), 0) /
        sessions.filter((s) => s.overallScore !== undefined).length
      : 0,
  };

  return (
    <div>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: 32 }}>
        <div>
          <Title level={2} style={{ color: '#fff', margin: 0 }}>
            <TrophyOutlined style={{ color: '#6C63FF', marginRight: 12 }} />
            Interview History
          </Title>
          <Text style={{ color: 'rgba(255,255,255,0.5)' }}>
            {stats.total} interviews · {stats.completed} completed ·
            {stats.avgScore > 0 ? ` Avg ${stats.avgScore.toFixed(1)}/10` : ' No scores yet'}
          </Text>
        </div>
        <Button
          type="primary"
          icon={<PlayCircleOutlined />}
          style={{ background: 'linear-gradient(135deg, #6C63FF, #48c6ef)', border: 'none', borderRadius: 8 }}
          onClick={() => navigate('/interview/new')}
        >
          New Interview
        </Button>
      </div>

      <Card style={{
        background: 'rgba(255,255,255,0.02)',
        border: '1px solid rgba(255,255,255,0.07)',
        borderRadius: 20,
      }}>
        {/* Filters */}
        <div style={{ display: 'flex', gap: 12, marginBottom: 20, flexWrap: 'wrap' }}>
          <Input
            prefix={<SearchOutlined style={{ color: '#6C63FF' }} />}
            placeholder="Search by role or title..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            style={{
              width: 280,
              background: 'rgba(255,255,255,0.05)',
              border: '1px solid rgba(255,255,255,0.1)',
              borderRadius: 8,
            }}
          />
          <Select
            value={statusFilter}
            onChange={setStatusFilter}
            style={{ width: 160 }}
          >
            {['All', 'Created', 'InProgress', 'Completed', 'Abandoned'].map((s) => (
              <Option key={s} value={s}>{s}</Option>
            ))}
          </Select>
        </div>

        {loading ? (
          <div style={{ textAlign: 'center', padding: 60 }}>
            <Spin size="large" />
          </div>
        ) : filtered.length === 0 ? (
          <Empty
            description={<Text style={{ color: 'rgba(255,255,255,0.4)' }}>No interviews found</Text>}
          />
        ) : (
          <Table
            dataSource={filtered}
            columns={columns}
            rowKey="id"
            pagination={{ pageSize: 10 }}
            scroll={{ x: 600 }}
          />
        )}
      </Card>
    </div>
  );
}
