import { useEffect, useState, useRef } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
  Card, Button, Typography, Progress, Space, Tag, Spin, Modal,
  Row, Col, Tooltip, message, Input, Divider, Alert,
} from 'antd';
import {
  AudioOutlined, AudioMutedOutlined, SoundOutlined,
  SendOutlined, LoadingOutlined,
  ClockCircleOutlined, QuestionCircleOutlined,
} from '@ant-design/icons';
import { interviewApi } from '../api/interview.api';
import { useSpeechRecognition } from '../hooks/useSpeechRecognition';
import { useSpeechSynthesis } from '../hooks/useSpeechSynthesis';
import type { Question, Answer } from '../types';

const { Title, Text, Paragraph } = Typography;
const { TextArea } = Input;

export function InterviewRoomPage() {
  const { id: sessionId } = useParams<{ id: string }>();
  const navigate = useNavigate();

  const [questions, setQuestions] = useState<Question[]>([]);
  const [currentIndex, setCurrentIndex] = useState(0);
  const [answers, setAnswers] = useState<Record<string, Answer>>({});
  const [textAnswer, setTextAnswer] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isLoading, setIsLoading] = useState(true);
  const [sessionStarted, setSessionStarted] = useState(false);
  const [elapsedSeconds, setElapsedSeconds] = useState(0);
  const [useVoice, setUseVoice] = useState(false);
  const [showCompleteModal, setShowCompleteModal] = useState(false);

  const timerRef = useRef<ReturnType<typeof setInterval> | undefined>(undefined);
  const questionTimerRef = useRef<ReturnType<typeof setInterval> | undefined>(undefined);
  const [questionSeconds, setQuestionSeconds] = useState(0);

  const { transcript, isListening, isSupported, startListening, stopListening, resetTranscript } =
    useSpeechRecognition();
  const { speak, isSpeaking } = useSpeechSynthesis();

  // Load questions
  useEffect(() => {
    if (!sessionId) return;
    interviewApi.getQuestions(sessionId)
      .then((qs) => {
        setQuestions(qs);
        setIsLoading(false);
      })
      .catch(() => {
        message.error('Failed to load questions');
        setIsLoading(false);
      });
  }, [sessionId]);

  // Start session timer
  useEffect(() => {
    if (sessionStarted) {
      timerRef.current = setInterval(() => setElapsedSeconds((s) => s + 1), 1000);
      return () => clearInterval(timerRef.current);
    }
  }, [sessionStarted]);

  // Per-question timer
  useEffect(() => {
    setQuestionSeconds(0);
    if (sessionStarted) {
      questionTimerRef.current = setInterval(() => setQuestionSeconds((s) => s + 1), 1000);
      return () => clearInterval(questionTimerRef.current);
    }
  }, [currentIndex, sessionStarted]);

  const handleStartSession = async () => {
    try {
      await interviewApi.start(sessionId!);
      setSessionStarted(true);
      // Auto-read first question
      if (questions[0]) {
        setTimeout(() => {
          speak(`Question 1. ${questions[0].questionText}`, () => {
            if (useVoice) startListening();
          });
        }, 500);
      }
    } catch {
      message.error('Failed to start session');
    }
  };

  // Sync voice transcript to text area
  useEffect(() => {
    if (transcript && useVoice) {
      setTextAnswer(transcript);
    }
  }, [transcript, useVoice]);

  const handleSubmitAnswer = async () => {
    const currentQ = questions[currentIndex];
    const answerText = useVoice && transcript ? transcript : textAnswer;

    if (!answerText.trim()) {
      message.warning('Please provide an answer before submitting.');
      return;
    }

    setIsSubmitting(true);
    if (isListening) stopListening();

    try {
      const answer = await interviewApi.submitAnswer({
        questionId: currentQ.id,
        sessionId: sessionId!,
        answerText,
        isVoiceAnswer: useVoice,
        transcript: useVoice ? transcript : undefined,
        durationSeconds: questionSeconds,
      });

      setAnswers((prev) => ({ ...prev, [currentQ.id]: answer }));

      // Move to next or complete
      if (currentIndex < questions.length - 1) {
        const nextIndex = currentIndex + 1;
        setCurrentIndex(nextIndex);
        setTextAnswer('');
        resetTranscript();

        // Read next question
        setTimeout(() => {
          speak(`Question ${nextIndex + 1}. ${questions[nextIndex].questionText}`, () => {
            if (useVoice) startListening();
          });
        }, 300);
      } else {
        setShowCompleteModal(true);
      }
    } catch {
      message.error('Failed to submit answer. Please try again.');
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleCompleteInterview = async () => {
    try {
      await interviewApi.complete(sessionId!);
      navigate(`/interview/${sessionId}/results`);
    } catch {
      message.error('Failed to complete interview');
    }
  };

  const formatTime = (secs: number) => {
    const m = Math.floor(secs / 60).toString().padStart(2, '0');
    const s = (secs % 60).toString().padStart(2, '0');
    return `${m}:${s}`;
  };

  if (isLoading) return (
    <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: 400 }}>
      <Spin size="large" />
    </div>
  );

  const currentQ = questions[currentIndex];
  const progress = ((currentIndex + (answers[currentQ?.id] ? 1 : 0)) / questions.length) * 100;
  const answeredCount = Object.keys(answers).length;

  if (!sessionStarted) {
    return (
      <div style={{ maxWidth: 700, margin: '0 auto' }}>
        <Card style={{
          background: 'rgba(255,255,255,0.03)',
          border: '1px solid rgba(255,255,255,0.07)',
          borderRadius: 20,
          textAlign: 'center',
          padding: '48px 32px',
        }}>
          <div style={{
            width: 80, height: 80, borderRadius: '50%',
            background: 'linear-gradient(135deg, #6C63FF, #48c6ef)',
            display: 'flex', alignItems: 'center', justifyContent: 'center',
            margin: '0 auto 24px', fontSize: 36,
          }}>
            🎤
          </div>
          <Title level={2} style={{ color: '#fff' }}>Ready to Begin?</Title>
          <Paragraph style={{ color: 'rgba(255,255,255,0.6)', fontSize: 16, maxWidth: 480, margin: '0 auto 32px' }}>
            You have <strong style={{ color: '#6C63FF' }}>{questions.length} questions</strong> to answer.
            Take your time, be specific, and think aloud. The AI will evaluate each response instantly.
          </Paragraph>

          <Space direction="vertical" size={16} style={{ width: '100%', maxWidth: 360, margin: '0 auto' }}>
            {isSupported && (
              <div style={{
                padding: '12px 20px',
                background: 'rgba(108,99,255,0.1)',
                border: '1px solid rgba(108,99,255,0.2)',
                borderRadius: 12,
                display: 'flex', justifyContent: 'space-between', alignItems: 'center',
              }}>
                <Text style={{ color: '#fff' }}>🎙️ Voice Mode</Text>
                <Button
                  size="small"
                  type={useVoice ? 'primary' : 'default'}
                  onClick={() => setUseVoice(!useVoice)}
                  style={useVoice ? { background: '#6C63FF', border: 'none' } : {}}
                >
                  {useVoice ? 'On' : 'Off'}
                </Button>
              </div>
            )}

            <Button
              id="start-interview-btn"
              type="primary"
              size="large"
              block
              style={{
                height: 54, fontSize: 16, fontWeight: 700,
                background: 'linear-gradient(135deg, #6C63FF, #48c6ef)',
                border: 'none', borderRadius: 12,
                boxShadow: '0 8px 32px rgba(108,99,255,0.4)',
              }}
              onClick={handleStartSession}
            >
              Start Interview 🚀
            </Button>
          </Space>
        </Card>
      </div>
    );
  }

  return (
    <div style={{ maxWidth: 900, margin: '0 auto' }}>
      {/* Header */}
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 20 }}>
        <Space>
          <Tag color="blue" icon={<ClockCircleOutlined />}>{formatTime(elapsedSeconds)}</Tag>
          <Text style={{ color: 'rgba(255,255,255,0.5)' }}>
            Question {currentIndex + 1} of {questions.length}
          </Text>
        </Space>
        <Space>
          <Tag color={isSpeaking ? 'purple' : 'default'} icon={<SoundOutlined />}>
            {isSpeaking ? 'Speaking...' : 'AI Ready'}
          </Tag>
          <Button
            size="small"
            danger
            onClick={() => navigate('/interview/history')}
          >
            Exit
          </Button>
        </Space>
      </div>

      {/* Progress */}
      <Progress
        percent={progress}
        strokeColor={{ from: '#6C63FF', to: '#48c6ef' }}
        trailColor="rgba(255,255,255,0.06)"
        style={{ marginBottom: 24 }}
        format={() => `${answeredCount}/${questions.length}`}
      />

      {/* Question Card */}
      <Card style={{
        background: 'rgba(255,255,255,0.03)',
        border: '1px solid rgba(108,99,255,0.2)',
        borderRadius: 20,
        marginBottom: 20,
      }}>
        <Row gutter={16} align="middle" style={{ marginBottom: 16 }}>
          <Col>
            <Tag color="purple">{currentQ?.category}</Tag>
            <Tag color="blue">{currentQ?.skillTag}</Tag>
          </Col>
          <Col flex="auto" style={{ textAlign: 'right' }}>
            <Text style={{ color: 'rgba(255,255,255,0.4)', fontSize: 12 }}>
              <ClockCircleOutlined style={{ marginRight: 4 }} />
              {formatTime(questionSeconds)}
            </Text>
          </Col>
        </Row>

        <Paragraph style={{
          color: '#fff',
          fontSize: 20,
          fontWeight: 600,
          lineHeight: 1.6,
          margin: '0 0 12px',
        }}>
          {currentQ?.questionText}
        </Paragraph>

        <Tooltip title="Listen to question">
          <Button
            type="text"
            icon={<SoundOutlined />}
            size="small"
            style={{ color: '#6C63FF' }}
            onClick={() => speak(currentQ?.questionText || '')}
            loading={isSpeaking}
          >
            Read aloud
          </Button>
        </Tooltip>
      </Card>

      {/* Answer Section */}
      <Card style={{
        background: 'rgba(255,255,255,0.02)',
        border: '1px solid rgba(255,255,255,0.07)',
        borderRadius: 20,
      }}>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 16 }}>
          <Text style={{ color: 'rgba(255,255,255,0.7)', fontWeight: 600 }}>
            Your Answer
          </Text>
          {isSupported && (
            <Space>
              <Tag color={isListening ? 'red' : 'default'}>
                {isListening ? '🔴 Recording' : '⚪ Not recording'}
              </Tag>
              <Button
                type={isListening ? 'primary' : 'default'}
                danger={isListening}
                icon={isListening ? <AudioMutedOutlined /> : <AudioOutlined />}
                onClick={() => {
                  if (isListening) {
                    stopListening();
                    setUseVoice(false);
                  } else {
                    setUseVoice(true);
                    resetTranscript();
                    startListening();
                  }
                }}
                style={isListening ? { background: '#EF4444', border: 'none' } : {}}
              >
                {isListening ? 'Stop Recording' : 'Voice Answer'}
              </Button>
            </Space>
          )}
        </div>

        {isListening && (
          <Alert
            message="Listening... Speak your answer clearly"
            type="info"
            icon={<LoadingOutlined />}
            style={{
              background: 'rgba(108,99,255,0.1)',
              border: '1px solid rgba(108,99,255,0.3)',
              marginBottom: 12, borderRadius: 8,
            }}
          />
        )}

        <TextArea
          id="answer-textarea"
          value={useVoice && transcript ? transcript : textAnswer}
          onChange={(e) => {
            if (!useVoice) setTextAnswer(e.target.value);
          }}
          rows={6}
          placeholder="Type your answer here, or use the Voice Answer button to speak..."
          style={{
            background: 'rgba(255,255,255,0.04)',
            border: '1px solid rgba(255,255,255,0.1)',
            borderRadius: 12,
            color: '#fff',
            resize: 'none',
            fontSize: 15,
          }}
          readOnly={isListening}
        />

        <Divider style={{ borderColor: 'rgba(255,255,255,0.07)' }} />

        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
          <Text style={{ color: 'rgba(255,255,255,0.3)', fontSize: 12 }}>
            <QuestionCircleOutlined /> Your answer is evaluated instantly by AI
          </Text>

          <Button
            id="submit-answer-btn"
            type="primary"
            icon={isSubmitting ? <LoadingOutlined /> : <SendOutlined />}
            loading={isSubmitting}
            size="large"
            onClick={handleSubmitAnswer}
            disabled={isSubmitting}
            style={{
              background: 'linear-gradient(135deg, #6C63FF, #48c6ef)',
              border: 'none',
              borderRadius: 10,
              fontWeight: 600,
            }}
          >
            {currentIndex < questions.length - 1 ? 'Submit & Next' : 'Submit & Finish'}
          </Button>
        </div>
      </Card>

      {/* Complete Modal */}
      <Modal
        open={showCompleteModal}
        onOk={handleCompleteInterview}
        onCancel={() => setShowCompleteModal(false)}
        title={<Text style={{ color: '#fff', fontSize: 18 }}>Interview Complete! 🎉</Text>}
        okText="View Results"
        okButtonProps={{
          style: { background: 'linear-gradient(135deg, #6C63FF, #48c6ef)', border: 'none' },
        }}
        styles={{ mask: { backdropFilter: 'blur(8px)' } }}
      >
        <div style={{ textAlign: 'center', padding: '24px 0' }}>
          <div style={{ fontSize: 64, marginBottom: 16 }}>🏆</div>
          <Paragraph style={{ fontSize: 16 }}>
            You've completed all <strong>{questions.length} questions</strong>!
            The AI is evaluating your answers — results include detailed feedback,
            skill gap analysis, and a personalized study roadmap.
          </Paragraph>
        </div>
      </Modal>
    </div>
  );
}
