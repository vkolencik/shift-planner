import React, { FC } from 'react'
import { Col, Container, Row } from 'react-bootstrap'

export const Footer: FC = () =>
  <footer className='footer pl-3 text-bg-light w-100 d-none d-sm-block mt-auto sticky-bottom border-top'>
    <Container fluid color='secondary'>
      <Row>
        <Col className='border-end col-auto' key='about'>
          <a href='https://krtek.me/info'>
            O aplikaci
          </a>
        </Col>
        <Col key='contact'>
          V případě problémů nás prosím kontaktujte na <a href='mailto:help@krtek.me'>help@krtek.me</a>
        </Col>
      </Row>
    </Container>
  </footer>
